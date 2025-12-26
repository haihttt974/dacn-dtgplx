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

    // =====================================================
    // GOOGLE
    // =====================================================

    [HttpGet]
    public IActionResult GoogleLogin(string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), "ExternalAuth", new { returnUrl })
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
    {
        return await ExternalLoginCallback(
            GoogleDefaults.AuthenticationScheme,
            returnUrl
        );
    }

    // =====================================================
    // FACEBOOK
    // =====================================================

    [HttpGet]
    public IActionResult FacebookLogin(string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(FacebookCallback), "ExternalAuth", new { returnUrl })
        };

        return Challenge(props, "Facebook");
    }

    [HttpGet]
    public async Task<IActionResult> FacebookCallback(string returnUrl = "/")
    {
        return await ExternalLoginCallback(
            "Facebook",
            returnUrl
        );
    }

    // =====================================================
    // COMMON HANDLER (GOOGLE + FACEBOOK)
    // =====================================================

    private async Task<IActionResult> ExternalLoginCallback(
        string scheme,
        string returnUrl
    )
    {
        var result = await HttpContext.AuthenticateAsync(scheme);

        if (!result.Succeeded)
        {
            TempData["Error"] = "Đăng nhập thất bại";
            return RedirectToAction("Login", "Auth");
        }

        // ===== LẤY CLAIM =====
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Không lấy được email từ tài khoản ngoài";
            return RedirectToAction("Login", "Auth");
        }

        // ===== TÌM USER =====
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        // ===== CHƯA CÓ → TẠO MỚI =====
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Username = email,
                TenDayDu = name,
                TrangThai = true,
                LaGiaoVien = false,
                RoleId = 2, // Học viên
                Password = "",
                LanDangNhapGanNhat = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            user.LanDangNhapGanNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ===== COOKIE LOGIN =====
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

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
