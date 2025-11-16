using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    [Route("admin/courses")]
    public class AdminCoursesController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminCoursesController(DtGplxContext context)
        {
            _context = context;
        }

        [HttpGet("")]
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
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Hangs = _context.Hangs.ToList();
            return View();
        }

        // POST: /admin/courses/create
        [HttpPost("create")]
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

        [HttpGet("details/{id:int}")]
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

        [HttpGet("edit/{id:int}")]
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

        [HttpPost("edit/{id:int}")]
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

        [HttpGet("schedule/{id:int}")]
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

        [HttpGet("schedule/add/{khoaHocId:int}")]
        public async Task<IActionResult> AddSchedule(int khoaHocId)
        {
            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.LichHocs)
                .FirstOrDefaultAsync(k => k.KhoaHocId == khoaHocId);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhoaHoc = khoaHoc;

            // Dropdown hình thức
            ViewBag.HinhThucList = new List<string> { "Lý thuyết", "Mô phỏng", "Thực hành" };

            // Xe rảnh
            ViewBag.XeList = await _context.XeTapLais
                .Where(x => x.TrangThaiXe == true)
                .ToListAsync();

            // Lớp rảnh
            ViewBag.LopList = await _context.LopHocs
                .Where(l => l.TrangThaiLop == true)
                .ToListAsync();

            return View(khoaHoc); // Model chính là KhoaHoc
        }

        [HttpPost("schedule/add/{khoaHocId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule(
            int khoaHocId,
            LichHoc model,
            string hinhThuc)
        {
            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.LichHocs)
                .FirstOrDefaultAsync(k => k.KhoaHocId == khoaHocId);

            if (khoaHoc == null)
            {
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // -----------------------------
            // 1) VALIDATE NGÀY HỌC
            // -----------------------------
            if (model.NgayHoc < DateOnly.FromDateTime(khoaHoc.NgayBatDau.Value) ||
                model.NgayHoc > DateOnly.FromDateTime(khoaHoc.NgayKetThuc.Value))
            {
                TempData["Error"] = "Ngày học phải nằm trong phạm vi khóa học.";
                return Redirect($"/admin/courses/schedule/add/{khoaHocId}");
            }

            // -----------------------------
            // 2) VALIDATE GIỜ
            // -----------------------------
            if (model.TgBatDau >= model.TgKetThuc)
            {
                TempData["Error"] = "Giờ bắt đầu phải nhỏ hơn giờ kết thúc.";
                return Redirect($"/admin/courses/schedule/add/{khoaHocId}");
            }

            // -----------------------------
            // 3) CHECK BUỔI (Không học 2 lần trong 1 buổi)
            // -----------------------------
            string buoiMoi = (model.TgBatDau.Hour < 12) ? "Sang" : "Chieu";

            var lichTrongNgay = khoaHoc.LichHocs
                .Where(l => l.NgayHoc == model.NgayHoc)
                .ToList();

            foreach (var lich in lichTrongNgay)
            {
                string buoiDaCo = (lich.TgBatDau.Hour < 12) ? "Sang" : "Chieu";

                if (buoiMoi == buoiDaCo)
                {
                    TempData["Error"] = $"Buổi {buoiMoi} đã có lịch học rồi, vui lòng chọn buổi khác.";
                    return Redirect($"/admin/courses/schedule/add/{khoaHocId}");
                }
            }

            // -----------------------------
            // 4) KIỂM TRA XE / LỚP CÓ ĐANG BỊ TRÙNG GIỜ KHÔNG (TOÀN HỆ THỐNG)
            // -----------------------------
            var tatCaLich = await _context.LichHocs.ToListAsync();

            foreach (var lich in tatCaLich)
            {
                bool cungNgay = lich.NgayHoc == model.NgayHoc;

                bool giaoThoaThoiGian =
                    model.TgBatDau < lich.TgKetThuc &&
                    model.TgKetThuc > lich.TgBatDau;

                if (cungNgay && giaoThoaThoiGian)
                {
                    // Nếu là thực hành → check xe
                    if (hinhThuc == "Thực hành" && lich.XeTapLaiId == model.XeTapLaiId)
                    {
                        TempData["Error"] = "Xe này đã có lịch học trùng thời gian. Vui lòng chọn xe khác.";
                        return Redirect($"/admin/courses/schedule/add/{khoaHocId}");
                    }

                    // Nếu là lý thuyết/mô phỏng → check lớp
                    if ((hinhThuc == "Lý thuyết" || hinhThuc == "Mô phỏng")
                        && lich.LopHocId == model.LopHocId)
                    {
                        TempData["Error"] = "Lớp học này đã có lịch trùng thời gian. Vui lòng chọn lớp khác.";
                        return Redirect($"/admin/courses/schedule/add/{khoaHocId}");
                    }
                }
            }

            // -----------------------------
            // 5) GÁN ĐỊA ĐIỂM
            // -----------------------------
            switch (hinhThuc)
            {
                case "Thực hành":
                    model.DiaDiem = "Sân tập";
                    break;
                case "Lý thuyết":
                    model.DiaDiem = "Phòng A";
                    break;
                case "Mô phỏng":
                    model.DiaDiem = "Phòng M";
                    break;
            }

            // GÁN KHÓA HỌC
            model.KhoaHocId = khoaHocId;

            // Xét loại (chỉ 1 trong 2 được phép null)
            if (hinhThuc == "Thực hành")
                model.LopHocId = null;
            else
                model.XeTapLaiId = null;

            _context.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm lịch học thành công!";
            return Redirect($"/admin/courses/schedule/{khoaHocId}");
        }

        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability(
            DateOnly ngayHoc,
            TimeOnly tgBatDau,
            TimeOnly tgKetThuc,
            string hinhThuc)
        {
            // Lấy lịch theo ngày
            var lichTrongNgay = await _context.LichHocs
                .Where(l => l.NgayHoc == ngayHoc)
                .ToListAsync();

            // Danh sách bị khóa theo giờ trùng
            List<int> xeBlocked = new();
            List<int> lopBlocked = new();

            // ==== 1. KIỂM TRA XE/LỚP TRÙNG GIỜ ======================================
            foreach (var l in lichTrongNgay)
            {
                bool giaoThoa = tgBatDau < l.TgKetThuc && tgKetThuc > l.TgBatDau;

                if (giaoThoa)
                {
                    if (l.XeTapLaiId.HasValue)
                        xeBlocked.Add(l.XeTapLaiId.Value);

                    if (l.LopHocId.HasValue)
                        lopBlocked.Add(l.LopHocId.Value);
                }
            }

            // ==== 2. KIỂM TRA BUỔI (SÁNG – CHIỀU – TỐI) =============================

            string GetBuoi(TimeOnly gio)
            {
                if (gio.Hour < 12) return "Sáng";
                if (gio.Hour < 18) return "Chiều";
                return "Tối";
            }

            var buoiMoi = GetBuoi(tgBatDau);

            bool buoiBlocked = lichTrongNgay
                .Any(x => GetBuoi(x.TgBatDau) == buoiMoi);

            // Trả về Ajax
            return Json(new
            {
                buoiBlocked = buoiBlocked,
                xeBlocked = xeBlocked,
                lopBlocked = lopBlocked
            });
        }
    }
}
