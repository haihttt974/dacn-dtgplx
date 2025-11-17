using dacn_dtgplx.Hubs;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// View Renderer (nếu bạn dùng gửi email template)
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

// Session + HttpContext
builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<AutoUpdateKhoaHocService>();

builder.Services.AddSignalR();
builder.Services.AddHostedService<OnlineUserMonitor>();
// Cookie Auth (dùng cho web + SignalR)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
    });

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

// Swagger (nếu dùng API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseRouting();

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
