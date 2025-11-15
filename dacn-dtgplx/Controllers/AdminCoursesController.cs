using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    [Route("admin/[controller]/[action]")]
    public class AdminCoursesController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminCoursesController(DtGplxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search, int? hang, int? status, int page = 1)
        {
            int pageSize = 15;

            var query = _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .Include(k => k.DangKyHocs)
                .OrderBy(k => k.KhoaHocId) // Sắp xếp 1→150
                .AsQueryable();

            // Lọc
            if (!string.IsNullOrEmpty(search))
                query = query.Where(k => k.TenKhoaHoc.Contains(search));

            if (hang.HasValue)
                query = query.Where(k => k.IdHang == hang.Value);

            if (status.HasValue)
                query = query.Where(k => k.IsActive == (status == 1));

            // Phân trang
            int totalItems = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thống kê khóa học theo từng hạng
            var thongKe = await _context.KhoaHocs
                .GroupBy(k => k.IdHangNavigation.MaHang)
                .Select(g => new { Hang = g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.Hang, x => x.SoLuong);

            ViewBag.Hangs = await _context.Hangs.ToListAsync();
            ViewBag.ThongKe = thongKe;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Nếu là AJAX → trả về partial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CoursesTable", data);

            return View(data);
        }

        // GET: /admin/courses/create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Hangs = _context.Hangs.ToList();
            return View();
        }

        // POST: /admin/courses/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhoaHoc model, int SoNgayHoc)
        {
            ViewBag.Hangs = await _context.Hangs.ToListAsync();

            // EF không cần IdHangNavigation --> remove lỗi
            ModelState.Remove("IdHangNavigation");

            // --- VALIDATE BẮT BUỘC ---
            if (model.IdHang <= 0)
            {
                TempData["Error"] = "Vui lòng chọn hạng GPLX.";
                return View(model);
            }

            if (model.SlToiDa == null || model.SlToiDa <= 0)
            {
                TempData["Error"] = "Vui lòng nhập số lượng tối đa hợp lệ.";
                return View(model);
            }

            if (model.NgayBatDau == null)
            {
                TempData["Error"] = "Vui lòng chọn ngày bắt đầu học.";
                return View(model);
            }

            if (SoNgayHoc <= 0)
            {
                TempData["Error"] = "Số ngày học phải lớn hơn 0.";
                return View(model);
            }

            // Tính ngày kết thúc
            var ngayBatDau = model.NgayBatDau.Value;
            var ngayKetThuc = ngayBatDau.AddDays(SoNgayHoc);

            // --- LẤY THÔNG TIN HẠNG ---
            var hang = await _context.Hangs.FirstOrDefaultAsync(h => h.IdHang == model.IdHang);
            if (hang == null)
            {
                TempData["Error"] = "Không tìm thấy hạng GPLX.";
                return View(model);
            }

            // --- TÌM KHÓA MỚI NHẤT THEO HẠNG ---
            var lastCourse = await _context.KhoaHocs
                .Where(k => k.IdHang == model.IdHang)
                .OrderByDescending(k => k.KhoaHocId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastCourse != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(lastCourse.TenKhoaHoc, @"K(\d+)$");
                if (match.Success)
                    nextNumber = int.Parse(match.Groups[1].Value) + 1;
            }

            // --- TẠO TÊN KHÓA TỰ ĐỘNG ---
            string tenKhoaHocFull = $"Khóa học lái xe {hang.TenDayDu} – K{nextNumber:00}";

            // --- TẠO OBJECT LƯU DATABASE ---
            var khoaHoc = new KhoaHoc
            {
                TenKhoaHoc = tenKhoaHocFull,
                IdHang = model.IdHang,
                SlToiDa = model.SlToiDa,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                MoTa = model.MoTa,
                IsActive = false
            };

            _context.Add(khoaHoc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Mã khóa học không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .Include(k => k.LichHocs)
                .Include(k => k.DangKyHocs)
                .FirstOrDefaultAsync(k => k.KhoaHocId == id);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            return View(khoaHoc);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Mã khóa học không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var model = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .FirstOrDefaultAsync(k => k.KhoaHocId == id);

            if (model == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            // Tính số ngày học (Ngày kết thúc - bắt đầu)
            if (model.NgayBatDau != null && model.NgayKetThuc != null)
            {
                ViewBag.SoNgayHoc = (model.NgayKetThuc.Value - model.NgayBatDau.Value).Days;
            }
            else
            {
                ViewBag.SoNgayHoc = 1; // mặc định
            }

            return View(model);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhoaHoc model, int SoNgayHoc)
        {
            ModelState.Remove("IdHangNavigation");

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.IdHangNavigation)
                .FirstOrDefaultAsync(k => k.KhoaHocId == id);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            // =======================
            //   VALIDATE CÁC TRƯỜNG
            // =======================

            if (model.SlToiDa == null || model.SlToiDa <= 0)
            {
                TempData["Error"] = "Số lượng tối đa phải lớn hơn 0.";
                return View(khoaHoc);
            }

            if (model.NgayBatDau == null)
            {
                TempData["Error"] = "Vui lòng chọn ngày bắt đầu.";
                return View(khoaHoc);
            }

            if (SoNgayHoc <= 0)
            {
                TempData["Error"] = "Số ngày học phải lớn hơn 0.";
                return View(khoaHoc);
            }

            // ============================
            //  TÍNH NGÀY KẾT THÚC MỚI
            // ============================
            var ngayBatDau = model.NgayBatDau.Value;
            var ngayKetThuc = ngayBatDau.AddDays(SoNgayHoc);

            // ============================
            //  KIỂM TRA LỊCH HỌC TRƯỚC KHI BẬT ACTIVE
            // ============================
            if (model.IsActive == true)
            {
                bool hasSchedule = await _context.LichHocs
                    .AnyAsync(lh => lh.KhoaHocId == khoaHoc.KhoaHocId);

                if (!hasSchedule)
                {
                    TempData["Error"] = "Không thể mở khóa học vì chưa có lịch học nào!";
                    return View(khoaHoc);
                }
            }

            // ============================
            //  CẬP NHẬT DỮ LIỆU
            // ============================
            khoaHoc.SlToiDa = model.SlToiDa;
            khoaHoc.MoTa = model.MoTa;
            khoaHoc.NgayBatDau = ngayBatDau;
            khoaHoc.NgayKetThuc = ngayKetThuc;
            khoaHoc.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/admin/courses/schedule/{id}")]
        public async Task<IActionResult> Schedule(int id)
        {
            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.LichHocs)
                .ThenInclude(l => l.XeTapLai)
                .Include(k => k.LichHocs)
                .ThenInclude(l => l.LopHoc)
                .FirstOrDefaultAsync(k => k.KhoaHocId == id);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            return View(khoaHoc);
        }
    }
}
