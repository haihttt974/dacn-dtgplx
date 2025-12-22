using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dacn_dtgplx.Models;

public class ExternalAuthController : Controller
{
    private readonly DtGplxContext _context;

    public ExternalAuthController(DtGplxContext context)
    {
        _context = context;
    }

    // 1️⃣ Gọi Google
    [HttpGet]
    public IActionResult GoogleLogin(string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(
                nameof(GoogleCallback),
                "ExternalAuth",
                new { returnUrl }
            )
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    // 2️⃣ Google trả về đây
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
    {
        var result = await HttpContext.AuthenticateAsync(
            GoogleDefaults.AuthenticationScheme
        );

        if (!result.Succeeded)
        {
            TempData["Error"] = "Đăng nhập Google thất bại";
            return RedirectToAction("Login", "Auth");
        }

        // ===== LẤY THÔNG TIN GOOGLE =====
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal.FindFirstValue(ClaimTypes.Name);
        var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (email == null)
        {
            TempData["Error"] = "Không lấy được email Google";
            return RedirectToAction("Login", "Auth");
        }

        // ===== TÌM USER TRONG DB =====
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        // ===== NẾU CHƯA CÓ → TẠO MỚI =====
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Username = email,
                TenDayDu = name,
                TrangThai = true,
                LaGiaoVien = false,
                RoleId = 2,
                Password = "",
                LanDangNhapGanNhat = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // ===== CLAIMS + COOKIE LOGIN =====
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        // ===== SESSION (GIỐNG LOGIN THƯỜNG) =====
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("FullName", user.TenDayDu ?? user.Username);
        HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);

        return LocalRedirect(returnUrl);
    }
}
