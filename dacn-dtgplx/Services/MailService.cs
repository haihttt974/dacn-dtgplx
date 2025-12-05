using dacn_dtgplx.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using MimeKit;
using MimeKit.Text;

namespace dacn_dtgplx.Services
{
    public interface IMailService
    {
        Task SendPaymentSuccessEmail(string to, string name, string course, decimal amount);
    }

    public class MailService : IMailService
    {
        private readonly IConfiguration _config;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IServiceProvider _serviceProvider;

        public MailService(
            IConfiguration config,
            IRazorViewEngine viewEngine,
            ITempDataDictionaryFactory tempDataFactory,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _viewEngine = viewEngine;
            _tempDataFactory = tempDataFactory;
            _serviceProvider = serviceProvider;
        }

        // ============================
        // Render Razor view thành HTML
        // ============================
        private async Task<string> RenderTemplateAsync<T>(string viewPath, T model)
        {
            // Tạo HttpContext ảo với DI container
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor()
            );

            await using var sw = new StringWriter();

            // 🔍 1) Lấy view từ đường dẫn tuyệt đối (GetView) hoặc tìm theo MVC convention (FindView)
            var viewResult = _viewEngine.GetView(executingFilePath: null, viewPath, isMainPage: true);

            if (!viewResult.Success)
            {
                // fallback: tìm view theo ViewEngine thông thường
                viewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: true);
            }

            if (!viewResult.Success)
                throw new InvalidOperationException($"Không tìm thấy view: {viewPath}");

            // 📌 2) Khởi tạo ViewData
            var viewData = new ViewDataDictionary<T>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            // 📌 3) Khởi tạo TempData từ Factory (KHÔNG NEW tay)
            var tempData = _tempDataFactory.GetTempData(httpContext);

            // 📌 4) Tạo ViewContext để render
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View!,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            // 📌 5) Render view → HTML string
            await viewResult.View!.RenderAsync(viewContext);

            return sw.ToString();
        }

        // =====================================
        // Gửi mail xác nhận thanh toán thành công
        // =====================================
        public async Task SendPaymentSuccessEmail(string to, string name, string course, decimal amount)
        {
            var model = new PaymentEmailVM
            {
                FullName = name,
                CourseName = course,
                Amount = amount
            };

            string htmlBody = await RenderTemplateAsync(
                "/Views/Templates/PaymentSuccess.cshtml",
                model
            );

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("DTGPLX Center", _config["Mail:From"]));
            email.To.Add(new MailboxAddress(name, to));
            email.Subject = $"Xác nhận thanh toán thành công - {course}";
            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Mail:From"], _config["Mail:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
