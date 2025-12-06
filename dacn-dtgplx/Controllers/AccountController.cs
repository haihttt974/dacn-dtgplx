using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

public class AccountController : Controller
{
    private readonly DtGplxContext _db;
    private readonly DashboardService _dashboardService;
    private readonly IWebHostEnvironment _env;

    public AccountController(
        DtGplxContext db,
        DashboardService dashboardService,
        IWebHostEnvironment env)
    {
        _db = db;
        _dashboardService = dashboardService;
        _env = env;
    }

    // ================= DASHBOARD (bạn đã có) =================
    public async Task<IActionResult> Dashboard()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var dto = await _dashboardService.GetUserDashboardAsync(userId.Value);
        return View(dto);
    }

    // ================= PROFILE - GET =================
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user == null) return NotFound();

        var vm = new UserProfileViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            TenDayDu = user.TenDayDu,
            Email = user.Email,
            SoDienThoai = user.SoDienThoai,
            DiaChi = user.DiaChi,
            Cccd = user.Cccd,
            GioiTinh = user.GioiTinh,
            NgaySinh = user.NgaySinh,
            Avatar = user.Avatar
        };

        return View(vm);
    }

    // ================= PROFILE - POST =================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UserProfileViewModel model, IFormFile? avatarFile)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user == null) return NotFound();

        // ----------------- Cập nhật thông tin text -----------------
        user.TenDayDu = model.TenDayDu;
        user.Email = model.Email;
        user.SoDienThoai = model.SoDienThoai;
        user.DiaChi = model.DiaChi;
        user.Cccd = model.Cccd;
        user.GioiTinh = model.GioiTinh;
        user.NgaySinh = model.NgaySinh;

        // ==========================
        //     XỬ LÝ AVATAR
        // ==========================
        if (avatarFile != null && avatarFile.Length > 0)
        {
            // 1. Chuẩn hóa tên để làm fileName
            string normalizedName = RemoveDiacritics(model.TenDayDu ?? user.Username)
                                        .ToLower()
                                        .Replace(" ", "")
                                        .Replace("đ", "d")
                                        .Replace("Đ", "D");

            string ext = Path.GetExtension(avatarFile.FileName);
            string fileName = $"{normalizedName}{ext}";

            string uploadFolder = Path.Combine(_env.WebRootPath, "images", "avatar");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            string fullPath = Path.Combine(uploadFolder, fileName);

            // 2. XÓA ẢNH CŨ NẾU TỒN TẠI
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                string oldAvatar = user.Avatar.Replace("wwwroot/", "");
                string oldFullPath = Path.Combine(_env.WebRootPath, oldAvatar.TrimStart('/'));

                if (System.IO.File.Exists(oldFullPath))
                    System.IO.File.Delete(oldFullPath);
            }

            // 3. LƯU ẢNH MỚI
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            // Lưu path đúng format DB yêu cầu
            user.Avatar = $"wwwroot/images/avatar/{fileName}";

            // Cập nhật ảnh hiển thị ở header
            HttpContext.Session.SetString("Avatar", "/images/avatar/" + fileName);
        }

        // =============================================================

        await _db.SaveChangesAsync();

        HttpContext.Session.SetString("FullName", user.TenDayDu ?? user.Username);

        TempData["Success"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction(nameof(Profile));
    }

    // ---------------------------------------
    // HÀM LOẠI BỎ DẤU TIẾNG VIỆT
    // ---------------------------------------
    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalized)
        {
            var unicodeCat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCat != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    // ============================================================
    //                      SETTINGS (GET)
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Login", "Auth");

        var user = await _db.Users
            .Include(r => r.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId.Value);

        if (user == null)
            return RedirectToAction("Login", "Auth");

        var vm = new UserSettingsViewModel
        {
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            TenDayDu = user.TenDayDu,
            RoleName = user.Role?.RoleName ?? "Người dùng",
            TrangThai = user.TrangThai,
            TaoLuc = user.TaoLuc,
            LanDangNhapGanNhat = user.LanDangNhapGanNhat,

            // Nếu bạn có bảng settings riêng → map tại đây
            AllowProfileVisibility = true,
            AllowEmailNotifications = true,
            AllowScheduleNotifications = true
        };

        return View(vm);
    }

    // ============================================================
    //                      LƯU CÀI ĐẶT (POST)
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> SaveSettings(UserSettingsViewModel model)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return Json(new { success = false, message = "Bạn chưa đăng nhập." });

        var user = await _db.Users.FindAsync(userId.Value);

        if (user == null)
            return Json(new { success = false, message = "Không tìm thấy người dùng." });

        // LƯU CÀI ĐẶT (tuỳ bạn lưu vào DB nào)
        // Nếu bạn có bảng UserSettings → lưu vào bảng đó
        // Nếu chưa có, để tạm thời session như sau:

        HttpContext.Session.SetString("AllowProfileVisibility", model.AllowProfileVisibility.ToString());
        HttpContext.Session.SetString("AllowEmailNotifications", model.AllowEmailNotifications.ToString());
        HttpContext.Session.SetString("AllowScheduleNotifications", model.AllowScheduleNotifications.ToString());

        return Json(new { success = true, message = "Đã lưu cài đặt thành công!" });
    }


    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return Json(new { success = false, message = "Bạn chưa đăng nhập." });

        var user = await _db.Users.FindAsync(userId.Value);
        if (user == null)
            return Json(new { success = false, message = "Không tìm thấy người dùng." });

        // ---------------- KIỂM TRA MẬT KHẨU ----------------
        if (string.IsNullOrWhiteSpace(currentPassword))
            return Json(new { success = false, message = "Vui lòng nhập mật khẩu hiện tại." });

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ mật khẩu mới." });

        if (newPassword != confirmPassword)
            return Json(new { success = false, message = "Mật khẩu mới và xác nhận không khớp nhau." });

        // ---------------- CẬP NHẬT ----------------
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return Json(new { success = true, redirect = Url.Action("Settings") });
    }

    // ============================================================
    //                     XÓA TÀI KHOẢN
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> DeleteAccount(UserSettingsViewModel model)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return Json(new { success = false, message = "Bạn chưa đăng nhập." });

        if (model.DeleteConfirmText != "XÓA TÀI KHOẢN")
        {
            return Json(new
            {
                success = false,
                message = "Chuỗi xác nhận không đúng!"
            });
        }

        var user = await _db.Users.FindAsync(userId.Value);

        if (user == null)
            return Json(new { success = false, message = "Không tìm thấy người dùng." });

        // NGUY HIỂM — xoá toàn bộ
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        // Xoá session
        HttpContext.Session.Clear();

        return Json(new { success = true, message = "Tài khoản đã được xóa vĩnh viễn." });
    }
}
