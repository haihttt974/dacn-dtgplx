using BCrypt.Net;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dacn_dtgplx.Controllers
{
    public class AuthController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly IViewRenderService _viewRender;   // <---

        public AuthController(DtGplxContext context, IConfiguration config, IViewRenderService viewRender) // <---
        {
            _context = context;
            _config = config;
            _viewRender = viewRender;                      // <---
            _emailService = new EmailService(config);
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                TempData["Error"] = "Sai tên đăng nhập hoặc mật khẩu!";
                return RedirectToAction("Login");
            }

            if (!user.TrangThai)
            {
                TempData["Warning"] = "Tài khoản của bạn đang bị khóa!";
                return RedirectToAction("Login");
            }

            // Update last login
            user.LanDangNhapGanNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);
            HttpContext.Session.SetString("JWTToken", token);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);

            TempData["Success"] = $"Đăng nhập thành công, chào {user.TenDayDu ?? user.Username}!";

            // Điều hướng layout
            if (user.RoleId == 1)
            {
                HttpContext.Session.SetString("Layout", "_LayoutAdmin");  // lưu layout admin
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                HttpContext.Session.SetString("Layout", "_Layout");       // layout người dùng thường
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string email, string tenDayDu)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                TempData["Error"] = "Tên đăng nhập đã tồn tại!";
                return RedirectToAction("Register");
            }

            var newUser = new User
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                TenDayDu = tenDayDu,
                TrangThai = true,
                LaGiaoVien = false,
                RoleId = 2, // Học viên
                TaoLuc = DateTime.UtcNow,
                CapNhatLuc = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Gửi email chào mừng bằng template Razor
            var htmlBody = await _viewRender.RenderToStringAsync(
                this,
                "~/Views/Templates/RegisterEmail.cshtml",
                newUser
            );
            await _emailService.SendEmailAsync(email, "Đăng ký thành công", htmlBody);

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET: /Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST: /Auth/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng với email này.";
                return RedirectToAction("ForgotPassword");
            }

            var newPass = Guid.NewGuid().ToString("N").Substring(0, 8);
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPass);
            await _context.SaveChangesAsync();

            var htmlBody = await _viewRender.RenderToStringAsync(
                this,
                "~/Views/Templates/ForgotPasswordEmail.cshtml",
                newPass
            );
            await _emailService.SendEmailAsync(email, "Khôi phục mật khẩu", htmlBody);

            TempData["Info"] = "Mật khẩu mới đã được gửi đến email của bạn.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Info"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim("roleId", user.RoleId?.ToString() ?? "0")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
