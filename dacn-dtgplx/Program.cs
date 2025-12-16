using dacn_dtgplx.Configs;
using dacn_dtgplx.Hubs;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1️⃣ Đăng ký Services
// =============================================

// Database
builder.Services.AddDbContext<DtGplxContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC + View + Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddSingleton<IQrService, QrService>();

// View Renderer (nếu bạn dùng gửi email template)
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

// Session + HttpContext
builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<AutoUpdateKhoaHocService>();
builder.Services.AddScoped<SteganographyService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddSignalR();
builder.Services.AddHostedService<OnlineUserMonitor>();
// Cookie Auth (dùng cho web + SignalR)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/Error/403";
    });
//
builder.Services.Configure<SteganographyOptions>(
    builder.Configuration.GetSection("Steganography"));
builder.Services.AddScoped<ISteganographyService, SteganographyService>();

// JWT Auth (dùng API)
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]))
        };
    });

builder.Services.AddAuthorization();

// ---- Load embeddings JSON ----
builder.Services.AddSingleton(provider =>
{
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    var filePath = Path.Combine(env.ContentRootPath, "PythonScripts", "questions_with_emb.json");

    var json = File.ReadAllText(filePath);

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    var items = JsonSerializer.Deserialize<List<QuestionEmbeddingVM>>(json, options);
    return items ?? new List<QuestionEmbeddingVM>();
});

// Đăng ký SteganographyService
builder.Services.AddSingleton<SteganographyService>();

// Swagger (nếu dùng API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<OnlineHub>("/onlineHub");
// =============================================
// 2️⃣ Middleware Pipeline
// =============================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}
// Middleware tùy chỉnh để vô hiệu hóa một số tính năng người dùng
//app.Use(async (context, next) =>
//{
//    var path = context.Request.Path.Value?.ToLower() ?? "";

//    if (path.Contains("vnpayreturn") ||
//        path.Contains("momoreturn") ||
//        path.Contains("paypalreturn"))
//    {
//        context.Items["DisableUserFeatures"] = true;
//    }

//    await next();
//});

app.UseHttpsRedirection();
app.UseStaticFiles();

//app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseSession();

// ---- THỨ TỰ BẮT BUỘC ----
app.UseAuthentication();
app.UseAuthorization();

// WebSocket (đang dùng cho realtime online)
app.UseWebSockets();
app.MapHub<OnlineHub>("/onlinehub");

// =============================================
// 3️⃣ Routing
// =============================================

// Nếu dùng API Controller (Auth, Online,...)
app.MapControllers();

// Route MVC mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
