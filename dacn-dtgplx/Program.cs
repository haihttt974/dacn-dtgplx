using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================
// 1️⃣  Add services
// ========================

// 🔹 Kết nối SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DtGplxContext>(options =>
    options.UseSqlServer(connectionString));

// 🔹 Thêm MVC (Controller + View)
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 🔹 Cấu hình JWT Authentication
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSession();

// ========================
// 2️⃣  Build app
// ========================
var app = builder.Build();

// ========================
// 3️⃣  Middleware pipeline
// ========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// ---- Thứ tự rất quan trọng ----
app.UseAuthentication();   // <== phải nằm trước Authorization
app.UseAuthorization();

// ========================
// 4️⃣  Map routes
// ========================

// API controllers (ví dụ AuthController)
app.MapControllers();

// MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
