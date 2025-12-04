using System.Security.Claims;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    [Authorize]
    [Route("khoahoc")]
    public class KhoaHocController : Controller
    {
        private readonly DtGplxContext _context;

        public KhoaHocController(DtGplxContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ============================================================
        // 1) INDEX – DANH SÁCH KHÓA HỌC
        // ============================================================
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Hangs = await _context.Hangs.ToListAsync();
            return View();
        }

        // ====================== LOAD COURSES ======================
        [AllowAnonymous]
        [HttpGet("load")]
        public async Task<IActionResult> LoadCourses(
            string? search,
            int? hang,
            DateOnly? ngay,
            bool? onlyHoSo,
            string? sort,
            int page = 1)
        {
            bool isLogged = User.Identity?.IsAuthenticated ?? false;
            int userId = isLogged ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : 0;

            var query = _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .Where(k => k.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(k => k.TenKhoaHoc.Contains(search));

            if (hang.HasValue)
                query = query.Where(k => k.IdHang == hang.Value);

            if (ngay.HasValue)
            {
                var dt = ngay.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(k => k.NgayBatDau <= dt && k.NgayKetThuc >= dt);
            }

            if (onlyHoSo == true && isLogged)
            {
                var loaiHoSo = await _context.HoSoThiSinhs
                    .Where(h => h.UserId == userId)
                    .Select(h => h.LoaiHoSo)
                    .ToListAsync();

                query = query.Where(k =>
                    loaiHoSo.Any(loai => loai.Contains(k.IdHangNavigation.MaHang)));
            }

            // nhớ using Microsoft.EntityFrameworkCore; ở đầu file

            query = sort switch
            {
                "start_desc" => query.OrderByDescending(k => k.NgayBatDau),
                "start_asc" => query.OrderBy(k => k.NgayBatDau),

                // Thời gian học dài nhất
                "duration_desc" => query.OrderByDescending(
                    k => EF.Functions.DateDiffDay(k.NgayBatDau!.Value, k.NgayKetThuc!.Value)
                ),

                // Thời gian học ngắn nhất
                "duration_asc" => query.OrderBy(
                    k => EF.Functions.DateDiffDay(k.NgayBatDau!.Value, k.NgayKetThuc!.Value)
                ),

                _ => query.OrderBy(k => k.KhoaHocId)
            };

            int pageSize = 12;
            int total = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return PartialView("_CourseList", data);
        }

        // ============================================================
        // 2) REGISTER – KIỂM TRA HỒ SƠ & HIỆN XÁC NHẬN
        // ============================================================
        [HttpGet("register/{id:int}")]
        public async Task<IActionResult> Register(int id)
        {
            int userId = GetUserId();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .FirstOrDefaultAsync(k => k.KhoaHocId == id);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var hoSo = await _context.HoSoThiSinhs
                .Where(h => h.UserId == userId)
                .Where(h => h.LoaiHoSo.Contains(khoaHoc.IdHangNavigation.MaHang))
                .FirstOrDefaultAsync();

            if (hoSo == null)
            {
                TempData["Error"] = "Bạn chưa có hồ sơ phù hợp với hạng của khóa học này.";
                return RedirectToAction(nameof(Index));
            }

            // Chuẩn bị popup xác nhận
            ViewBag.HoSoId = hoSo.HoSoId;
            ViewBag.ChiPhi = khoaHoc.IdHangNavigation.ChiPhi ?? 0;

            return View(khoaHoc);
        }

        // ============================================================
        // 3) CONFIRM REGISTER – LƯU VÀ CHUYỂN QUA THANH TOÁN
        // ============================================================
        [HttpPost("register/confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRegister(int khoaHocId, int hoSoId)
        {
            int userId = GetUserId();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .FirstOrDefaultAsync(k => k.KhoaHocId == khoaHocId);

            var hoSo = await _context.HoSoThiSinhs
                .FirstOrDefaultAsync(h => h.HoSoId == hoSoId && h.UserId == userId);

            if (khoaHoc == null || hoSo == null)
            {
                TempData["Error"] = "Đăng ký không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Tạo đăng ký mới
            var dk = new DangKyHoc
            {
                HoSoId = hoSo.HoSoId,
                KhoaHocId = khoaHoc.KhoaHocId,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = false
            };

            _context.Add(dk);
            await _context.SaveChangesAsync();

            // Chuyển sang trang thanh toán
            return RedirectToAction("StartPayment", "Payment", new { dangKyId = dk.IdDangKy });
        }
    }
}
