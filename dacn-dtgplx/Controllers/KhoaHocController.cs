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
            // Nếu user đã đăng nhập → kiểm tra hồ sơ
            if (User.Identity?.IsAuthenticated ?? false)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                bool hasHoSo = await _context.HoSoThiSinhs
                    .AnyAsync(h => h.UserId == userId);

                if (!hasHoSo)
                {
                    ViewBag.NoHoSoWarning = true;
                }
            }
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
                //var loaiHoSo = await _context.HoSoThiSinhs
                //    .Where(h => h.UserId == userId)
                //    .Select(h => h.LoaiHoSo)
                //    .ToListAsync();

                //query = query.Where(k =>
                //    loaiHoSo.Any(loai => loai.Contains(k.IdHangNavigation.MaHang)));
                query = query.Where(k =>
                _context.HoSoThiSinhs.Any(h =>
                    h.UserId == userId &&
                    // Nếu bạn chỉ muốn hồ sơ đã duyệt thì thêm dòng này:
                    // h.DaDuyet == true &&
                    h.LoaiHoSo.Contains(k.IdHangNavigation.MaHang)
                    )
                );
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
        public async Task<IActionResult> ConfirmRegister(int khoaHocId, int hoSoId, string? noiDung)
        {
            int userId = GetUserId();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .FirstOrDefaultAsync(k => k.KhoaHocId == khoaHocId);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction("Index");
            }

            // 1️⃣ TẠO ĐĂNG KÝ MỚI
            var dk = new DangKyHoc
            {
                HoSoId = hoSoId,
                KhoaHocId = khoaHocId,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = null,
                GhiChu = noiDung
            };

            _context.Add(dk);
            await _context.SaveChangesAsync();

            // 2️⃣ TẠO HÓA ĐƠN
            var hoaDon = new HoaDonThanhToan
            {
                IdDangKy = dk.IdDangKy,
                NgayThanhToan = null,
                TrangThai = null,
                SoTien = khoaHoc.IdHangNavigation.ChiPhi,
                NoiDung = noiDung,
                PhuongThucThanhToan = null
            };

            _context.Add(hoaDon);
            await _context.SaveChangesAsync();

            return RedirectToAction("StartPayment", "Payment", new { hoaDonId = hoaDon.IdThanhToan });
        }

        [AllowAnonymous]
        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var kh = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .Include(k => k.LichHocs)
                    .ThenInclude(l => l.LopHoc)
                .Include(k => k.LichHocs)
                    .ThenInclude(l => l.XeTapLai)
                .FirstOrDefaultAsync(k => k.KhoaHocId == id);

            if (kh == null)
            {
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction("Index");
            }

            return View(kh);
        }

        [HttpGet("my-courses")]
        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            int userId = GetUserId();

            var myCourses = await _context.DangKyHocs
                .Include(d => d.KhoaHoc)
                    .ThenInclude(k => k.IdHangNavigation)
                .Include(d => d.HoSo)
                .Where(d => d.HoSo.UserId == userId && d.TrangThai == true)
                .OrderByDescending(d => d.NgayDangKy)
                .ToListAsync();

            return View(myCourses);   // Views/KhoaHoc/MyCourses.cshtml
        }

        [Authorize]
        [HttpGet("schedule/{khoaHocId}")]
        public async Task<IActionResult> Schedule(int khoaHocId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Kiểm tra quyền xem
            bool hasAccess = await _context.DangKyHocs
                .AnyAsync(d => d.KhoaHocId == khoaHocId
                            && d.HoSo.UserId == userId
                            && d.TrangThai == true);

            if (!hasAccess)
            {
                TempData["Error"] = "Bạn không có quyền xem lịch học của khóa này.";
                return RedirectToAction("MyCourses");
            }

            // Lấy lịch học đầy đủ
            var lichHocs = await _context.LichHocs
                .Include(l => l.LopHoc)
                .Include(l => l.XeTapLai)
                .Where(l => l.KhoaHocId == khoaHocId)
                .OrderBy(l => l.NgayHoc)
                .ThenBy(l => l.TgBatDau)
                .ToListAsync();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .FirstAsync(k => k.KhoaHocId == khoaHocId);

            ViewBag.KhoaHoc = khoaHoc;

            return View("Schedule", lichHocs);
        }
    }
}
