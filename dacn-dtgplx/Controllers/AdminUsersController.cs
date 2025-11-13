using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace dacn_dtgplx.Controllers
{
    [Route("admin/users")]
    public class AdminUsersController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public AdminUsersController(
            DtGplxContext context,
            IWebHostEnvironment env,
            IConfiguration config,
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider)
        {
            _context = context;
            _env = env;
            _config = config;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
        }

        // ======================== INDEX ========================
        [HttpGet("")]
        public async Task<IActionResult> Index(int? roleId, string? q)
        {
            var users = _context.Users.Include(u => u.Role).AsQueryable();

            if (roleId.HasValue && roleId.Value > 0)
                users = users.Where(u => u.RoleId == roleId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                users = users.Where(u =>
                    (u.Email ?? "").ToLower().Contains(q) ||
                    (u.TenDayDu ?? "").ToLower().Contains(q) ||
                    u.Username.ToLower().Contains(q));
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            ViewBag.SearchKeyword = q;
            ViewBag.CurrentRoleId = roleId;

            var list = await users.OrderByDescending(u => u.CapNhatLuc).ToListAsync();
            return View(list);
        }

        // ====================== DETAILS ========================
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // ======================== EDIT =========================
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }

        // nhận thêm avatarFile từ form (name="AvatarFile")
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model, IFormFile? AvatarFile)
        {
            if (id != model.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                PushModelErrorsToTempData();
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(model);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.TenDayDu = model.TenDayDu;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.DiaChi = model.DiaChi;
            user.GioiTinh = model.GioiTinh;
            user.NgaySinh = model.NgaySinh;
            user.RoleId = model.RoleId;
            user.TrangThai = model.TrangThai;
            user.CapNhatLuc = DateTime.UtcNow;

            // =========== XỬ LÝ ẢNH KHI SỬA ===========
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                // 1. Xóa ảnh cũ nếu có và không phải default
                if (!string.IsNullOrWhiteSpace(user.Avatar) &&
                    !user.Avatar.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var avatarDbPath = user.Avatar.Replace("\\", "/");
                        string relative;

                        if (avatarDbPath.Contains("wwwroot", StringComparison.OrdinalIgnoreCase))
                        {
                            var idx = avatarDbPath.IndexOf("wwwroot", StringComparison.OrdinalIgnoreCase)
                                     + "wwwroot".Length;
                            relative = avatarDbPath.Substring(idx).TrimStart('/', '\\');
                        }
                        else
                        {
                            relative = avatarDbPath.TrimStart('/', '\\');
                        }

                        var oldPhysicalPath = Path.Combine(_env.WebRootPath,
                            relative.Replace("/", Path.DirectorySeparatorChar.ToString()));

                        if (System.IO.File.Exists(oldPhysicalPath))
                            System.IO.File.Delete(oldPhysicalPath);
                    }
                    catch { /* ignore delete errors */ }
                }

                // 2. Lưu ảnh mới với tên <username>.<ext>
                var username = user.Username; // username đã tồn tại
                var ext = Path.GetExtension(AvatarFile.FileName);
                if (string.IsNullOrEmpty(ext)) ext = ".png";

                var fileName = username + ext;

                var folderPhysical = Path.Combine(_env.WebRootPath, "images", "avatar");
                if (!Directory.Exists(folderPhysical))
                    Directory.CreateDirectory(folderPhysical);

                var newPhysicalPath = Path.Combine(folderPhysical, fileName);

                using (var stream = new FileStream(newPhysicalPath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                // Lưu vào DB: wwwroot/images/avatar/<username>.<ext>
                user.Avatar = Path
                    .Combine("wwwroot", "images", "avatar", fileName)
                    .Replace("\\", "/");
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ======================== CREATE ========================
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(new CreateUserViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();

            // ================== VALIDATE ==================
            if (string.IsNullOrWhiteSpace(vm.TenDayDu))
                ModelState.AddModelError("TenDayDu", "Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Email))
                ModelState.AddModelError("Email", "Email không được để trống.");
            else if (!IsValidEmail(vm.Email))
                ModelState.AddModelError("Email", "Email không hợp lệ.");
            else if (await _context.Users.AnyAsync(x => x.Email == vm.Email))
                ModelState.AddModelError("Email", "Email đã được sử dụng.");

            if (!IsValidPhone(vm.SoDienThoai))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại không hợp lệ (10 số).");

            if (!string.IsNullOrWhiteSpace(vm.SoDienThoai) &&
                await _context.Users.AnyAsync(x => x.SoDienThoai == vm.SoDienThoai))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng.");

            if (vm.RoleId == null || vm.RoleId <= 0)
                ModelState.AddModelError("RoleId", "Bạn phải chọn vai trò.");

            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "Bạn phải random mật khẩu trước khi tạo.");

            if (!ModelState.IsValid)
            {
                PushModelErrorsToTempData();
                return View(vm);
            }

            // ================== GENERATE USERNAME ==================
            string username = GenerateUsername(vm.TenDayDu);

            // ================== XỬ LÝ AVATAR KHI TẠO ==================
            string avatarPath;

            if (vm.AvatarFile != null && vm.AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.AvatarFile.FileName);
                if (string.IsNullOrEmpty(ext)) ext = ".png";

                var fileName = username + ext;

                var folderPhysical = Path.Combine(_env.WebRootPath, "images", "avatar");
                if (!Directory.Exists(folderPhysical))
                    Directory.CreateDirectory(folderPhysical);

                var physicalPath = Path.Combine(folderPhysical, fileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await vm.AvatarFile.CopyToAsync(stream);
                }

                avatarPath = Path
                    .Combine("wwwroot", "images", "avatar", fileName)
                    .Replace("\\", "/");
            }
            else
            {
                // dùng ảnh mặc định
                avatarPath = Path.Combine("wwwroot", "images", "avatar", "default.png")
                    .Replace("\\", "/");
            }

            // ================== HASH PASSWORD ==================
            string plainPass = vm.Password!;
            string hashedPass = BCrypt.Net.BCrypt.HashPassword(plainPass);

            // ================== INSERT DB ==================
            var newUser = new User
            {
                Username = username,
                Password = hashedPass,
                TenDayDu = vm.TenDayDu,
                Email = vm.Email,
                SoDienThoai = vm.SoDienThoai,
                RoleId = vm.RoleId,
                Avatar = avatarPath,
                TrangThai = true,
                TaoLuc = DateTime.UtcNow,
                CapNhatLuc = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // ================== SEND EMAIL ==================
            await SendCreateUserEmailAsync(new CreateUserEmailViewModel
            {
                FullName = newUser.TenDayDu ?? username,
                Email = newUser.Email!,
                Password = plainPass
            });

            TempData["Success"] = "Tạo người dùng thành công! Email thông báo đã được gửi.";
            return RedirectToAction(nameof(Index));
        }

        // ======================= HELPERS =========================

        private void PushModelErrorsToTempData()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            if (errors.Any())
                TempData["Errors"] = errors;
        }

        private static string GenerateUsername(string fullName)
        {
            string clean = RemoveDiacritics(fullName.ToLower().Trim());
            return string.Join(".", clean.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var unicode = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicode != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        }

        private bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true; // cho phép bỏ trống
            return Regex.IsMatch(phone, @"^(0[0-9]{9})$");
        }

        private async Task SendCreateUserEmailAsync(CreateUserEmailViewModel data)
        {
            try
            {
                var from = _config["Mail:From"];
                var pass = _config["Mail:Password"];

                string body = await RenderViewToStringAsync(
                    "~/Views/Templates/CreateUserEmail.cshtml", data);

                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(from, pass),
                    EnableSsl = true
                };

                var msg = new MailMessage
                {
                    From = new MailAddress(from, "Hệ thống GPLX"),
                    Subject = "Tài khoản của bạn đã được tạo",
                    Body = body,
                    IsBodyHtml = true
                };
                msg.To.Add(data.Email);

                await client.SendMailAsync(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<string> RenderViewToStringAsync(string viewPath, object model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

            using var sw = new StringWriter();

            var viewResult = _viewEngine.GetView(null, viewPath, false);
            if (!viewResult.Success)
                return $"Không tìm thấy view: {viewPath}";

            var vdd = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            { Model = model };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                vdd,
                new TempDataDictionary(HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
