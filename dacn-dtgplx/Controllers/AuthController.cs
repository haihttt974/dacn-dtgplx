using BCrypt.Net;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Authentication;
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
        private readonly AutoUpdateKhoaHocService _autoUpdate;

        public AuthController(DtGplxContext context, IConfiguration config, IViewRenderService viewRender, AutoUpdateKhoaHocService autoUpdate)
        {
            _context = context;
            _config = config;
            _viewRender = viewRender;
            _emailService = new EmailService(config);
            _autoUpdate = autoUpdate;
        }

        // ================================================================
        //                            LOGIN
        // ================================================================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null)
            {
                TempData["Error"] = "Sai tên đăng nhập hoặc Email!";
                return RedirectToAction("Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                TempData["Error"] = "Sai mật khẩu!";
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
            await _autoUpdate.UpdateKhoaHocStatusAsync();
            string avatarPath = user.Avatar ?? "";

            if (!string.IsNullOrWhiteSpace(avatarPath))
            {
                // đổi \ thành /
                avatarPath = avatarPath.Replace("\\", "/");

                // nếu bắt đầu bằng "wwwroot/" thì bỏ phần đó đi
                if (avatarPath.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                {
                    avatarPath = "/" + avatarPath.Substring("wwwroot/".Length);
                }
                // nếu không có dấu / đầu thì thêm vào
                else if (!avatarPath.StartsWith("/"))
                {
                    avatarPath = "/" + avatarPath;
                }
            }
            // JWT Token
            var token = GenerateJwtToken(user);
            HttpContext.Session.SetString("JWTToken", token);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);

            // ⭐ NEW: Lưu Avatar & FullName
            HttpContext.Session.SetString("Avatar", avatarPath);
            HttpContext.Session.SetString("FullName", user.TenDayDu ?? user.Username);
            //HttpContext.Session.SetString("UserId", user.UserId.ToString());
            //HttpContext.Session.SetString("Username", user.Username);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, (user.RoleId ?? 0).ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "local");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("Cookies", principal);
            // Set Online
            await MarkUserOnline(user.UserId);

            TempData["Success"] = $"Đăng nhập thành công, chào {user.TenDayDu ?? user.Username}!";

            // Điều hướng Role
            if (user.RoleId == 1)
            {
                HttpContext.Session.SetString("Layout", "_LayoutAdmin");
                return RedirectToAction("Index", "AdminDashboard");
            }

            HttpContext.Session.SetString("Layout", "_Layout");
            return RedirectToAction("Index", "Home");
        }

        // ================================================================
        //                            REGISTER
        // ================================================================
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
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                TempData["Error"] = "Tên đăng nhập đã tồn tại!";
                return RedirectToAction("Register");
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                TempData["Error"] = "Email đã được sử dụng!";
                return RedirectToAction("Register");
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("Register");
            }

            if (!IsStrongPassword(password))
            {
                TempData["Error"] = "Mật khẩu phải tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt!";
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
                RoleId = 2,
                TaoLuc = DateTime.UtcNow,
                CapNhatLuc = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Gửi email template Razor
            var htmlBody = await _viewRender.RenderToStringAsync(
                this,
                "~/Views/Templates/RegisterEmail.cshtml",
                newUser
            );

            await _emailService.SendEmailAsync(email, "Đăng ký thành công", htmlBody);

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ================================================================
        //                FORGOT PASSWORD — OTP 60s
        // ================================================================
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Vui lòng nhập email.";
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email.Trim());
            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return RedirectToAction("ForgotPassword");
            }

            // Tạo OTP 6 số
            string otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào Session (60s)
            HttpContext.Session.SetString("OTP_Code", otp);
            HttpContext.Session.SetString("OTP_Email", email.Trim());
            HttpContext.Session.SetString("OTP_Expire", DateTime.UtcNow.AddSeconds(60).ToString("O"));

            // Render template Razor cho email OTP
            var htmlBody = await _viewRender.RenderToStringAsync(
                this,
                "~/Views/Templates/ForgotPasswordEmail.cshtml",
                new
                {
                    OTP = otp,
                    Email = email.Trim(),
                    Username = user.TenDayDu ?? user.Username
                }
            );

            // Gửi email
            await _emailService.SendEmailAsync(
                email.Trim(),
                "Mã OTP xác thực đặt lại mật khẩu",
                htmlBody
            );

            TempData["Success"] = "Mã OTP đã được gửi tới email của bạn. Mã có hiệu lực trong 60 giây.";
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (HttpContext.Session.GetString("OTP_Email") == null)
                return RedirectToAction("ForgotPassword");

            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            string? code = HttpContext.Session.GetString("OTP_Code");
            string? expire = HttpContext.Session.GetString("OTP_Expire");

            if (code == null || expire == null)
                return RedirectToAction("ForgotPassword");

            if (DateTime.UtcNow > DateTime.Parse(expire))
            {
                TempData["Error"] = "Mã OTP đã hết hạn!";
                return RedirectToAction("VerifyOtp");
            }

            if (otp != code)
            {
                TempData["Error"] = "OTP không chính xác!";
                return RedirectToAction("VerifyOtp");
            }

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("OTP_Email") == null)
                return RedirectToAction("ForgotPassword");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("OTP_Email");

            if (email == null)
                return RedirectToAction("ForgotPassword");

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("ResetPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng!";
                return RedirectToAction("ForgotPassword");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Xóa Session
            HttpContext.Session.Remove("OTP_Code");
            HttpContext.Session.Remove("OTP_Email");
            HttpContext.Session.Remove("OTP_Expire");

            TempData["Success"] = "Đặt lại mật khẩu thành công!";
            return RedirectToAction("Login");
        }

        // ================================================================
        //                             LOGOUT
        // ================================================================
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
                await MarkUserOffline(userId.Value);

            HttpContext.Session.Clear();
            TempData["Info"] = "Đăng xuất thành công!";

            return RedirectToAction("Login");
        }

        // ================================================================
        //                        JWT GENERATOR
        // ================================================================
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

        // ================================================================
        //                   HELPER: PASSWORD CHECK
        // ================================================================
        private bool IsStrongPassword(string pass)
        {
            return pass.Length >= 8 &&
                   pass.Any(char.IsUpper) &&
                   pass.Any(char.IsLower) &&
                   pass.Any(char.IsDigit) &&
                   pass.Any(ch => !char.IsLetterOrDigit(ch));
        }

        // ================================================================
        //              HELPER: MARK USER ONLINE / OFFLINE
        // ================================================================
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
