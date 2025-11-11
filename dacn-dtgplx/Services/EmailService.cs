using System.Net;
using System.Net.Mail;

namespace dacn_dtgplx.Models
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Mail:From"],
                    _config["Mail:Password"])
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Mail:From"], "DT GPLX Support"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(to);
            await smtp.SendMailAsync(mail);
        }
    }
}
