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
        private readonly IViewRenderService _viewRender;

        public AuthController(DtGplxContext context, IConfiguration config, IViewRenderService viewRender)
        {
            _context = context;
            _config = config;
            _viewRender = viewRender;
            _emailService = new EmailService(config);
        }

        // ============================
        //       LOGIN
        // ============================
        [HttpGet]
        public IActionResult Login() => View();

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

            // Cập nhật lần đăng nhập
            user.LanDangNhapGanNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // ---- JWT ----
            var token = GenerateJwtToken(user);
            HttpContext.Session.SetString("JWTToken", token);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);

            // ---- ONLINE ----
            await MarkUserOnline(user.UserId);

            TempData["Success"] = $"Đăng nhập thành công, chào {user.TenDayDu ?? user.Username}!";

            // Điều hướng Role
            if (user.RoleId == 1)
            {
                HttpContext.Session.SetString("Layout", "_LayoutAdmin");
                return RedirectToAction("Index", "AdminDashboard");
            }
            else
            {
                HttpContext.Session.SetString("Layout", "_Layout");
                return RedirectToAction("Index", "Home");
            }
        }

        // ============================
        //       REGISTER
        // ============================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(
            string username,
            string email,
            string password,
            string confirmPassword,
            string tenDayDu)
        {
            // Kiểm tra username trùng
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                TempData["Error"] = "Tên đăng nhập đã tồn tại!";
                return RedirectToAction("Register");
            }

            // Kiểm tra email trùng
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                TempData["Error"] = "Email đã được sử dụng!";
                return RedirectToAction("Register");
            }

            // Check confirm password
            if (password != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("Register");
            }

            // Check password mạnh
            if (!IsStrongPassword(password))
            {
                TempData["Error"] = "Mật khẩu phải tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt!";
                return RedirectToAction("Register");
            }

            // Tạo user
            var newUser = new User
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                TenDayDu = tenDayDu,
                TrangThai = true,
                LaGiaoVien = false,
                RoleId = 2,
                TaoLuc = DateTime.UtcNow,
                CapNhatLuc = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Gửi email chào mừng
            var htmlBody = await _viewRender.RenderToStringAsync(
                this,
                "~/Views/Templates/RegisterEmail.cshtml",
                newUser
            );
            await _emailService.SendEmailAsync(email, "Đăng ký thành công", htmlBody);

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ============================
        //    FORGOT PASSWORD
        // ============================
        [HttpGet]
        public IActionResult ForgotPassword() => View();

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

        // ============================
        //          LOGOUT
        // ============================
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                await MarkUserOffline(userId.Value);
            }

            HttpContext.Session.Clear();
            TempData["Info"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        // ============================
        //      JWT Token Generator
        // ============================
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

        // ============================
        //      HELPER FUNCTIONS
        // ============================

        private bool IsStrongPassword(string pass)
        {
            return pass.Length >= 8 &&
                   pass.Any(char.IsUpper) &&
                   pass.Any(char.IsLower) &&
                   pass.Any(char.IsDigit) &&
                   pass.Any(ch => !char.IsLetterOrDigit(ch));
        }

        private async Task MarkUserOnline(int userId)
        {
            var now = DateTime.UtcNow;

            var conn = await _context.WebsocketConnections
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (conn == null)
            {
                conn = new WebsocketConnection
                {
                    UserId = userId,
                    ConnectedAt = now,
                    LastActivity = now,
                    IsOnline = true
                };
                _context.WebsocketConnections.Add(conn);
            }
            else
            {
                conn.LastActivity = now;
                conn.IsOnline = true;
            }

            await _context.SaveChangesAsync();
        }

        private async Task MarkUserOffline(int userId)
        {
            var conn = await _context.WebsocketConnections
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (conn != null)
            {
                conn.IsOnline = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
