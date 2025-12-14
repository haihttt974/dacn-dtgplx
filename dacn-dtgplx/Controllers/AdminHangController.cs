using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers.Admin
{
    [Authorize(Roles = "1")]
    [Route("Admin/Hang")]
    public class AdminHangController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminHangController(DtGplxContext context)
        {
            _context = context;
        }

        /* =====================================================
         * 1. DANH SÁCH HẠNG
         * ===================================================== */
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var hangs = await _context.Hangs
                .Include(h => h.QuyDinhHangs)
                .OrderBy(h => h.MaHang)
                .ToListAsync();

            return View(hangs);
        }

        /* =====================================================
         * 2. TẠO HẠNG
         * ===================================================== */
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hang model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.TaoLuc = DateTime.Now;

            _context.Hangs.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /* =====================================================
         * 3. SỬA HẠNG
         * ===================================================== */
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var hang = await _context.Hangs.FindAsync(id);
            if (hang == null) return NotFound();

            return View(hang);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hang model)
        {
            if (id != model.IdHang) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var hang = await _context.Hangs.FindAsync(id);
            if (hang == null) return NotFound();

            hang.MaHang = model.MaHang;
            hang.TenDayDu = model.TenDayDu;
            hang.MoTa = model.MoTa;
            hang.DiemDat = model.DiemDat;
            hang.ThoiGianTn = model.ThoiGianTn;
            hang.SoCauHoi = model.SoCauHoi;
            hang.TuoiToiThieu = model.TuoiToiThieu;
            hang.TuoiToiDa = model.TuoiToiDa;
            hang.SucKhoe = model.SucKhoe;
            hang.ChiPhi = model.ChiPhi;
            hang.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /* =====================================================
         * 4. QUY ĐỊNH HẠNG
         * ===================================================== */
        [HttpGet("{hangId}/QuyDinh")]
        public async Task<IActionResult> QuyDinh(int hangId)
        {
            var hang = await _context.Hangs
                .Include(h => h.QuyDinhHangs)
                .FirstOrDefaultAsync(h => h.IdHang == hangId);

            if (hang == null) return NotFound();

            var quyDinh = hang.QuyDinhHangs.FirstOrDefault()
                ?? new QuyDinhHang { IdHang = hangId };

            ViewBag.Hang = hang;
            return View(quyDinh);
        }

        [HttpPost("{hangId}/QuyDinh")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuyDinh(int hangId, QuyDinhHang model)
        {
            if (hangId != model.IdHang) return BadRequest();

            model.LyThuyet ??= false;
            model.SaHinh ??= false;
            model.MoPhong ??= false;
            model.DuongTruong ??= false;

            var quyDinh = await _context.QuyDinhHangs
                .FirstOrDefaultAsync(q => q.IdHang == hangId);

            if (quyDinh == null)
            {
                _context.QuyDinhHangs.Add(model);
            }
            else
            {
                quyDinh.KmToiThieu = model.KmToiThieu;
                quyDinh.SoGioBanDem = model.SoGioBanDem;
                quyDinh.LyThuyet = model.LyThuyet;
                quyDinh.SaHinh = model.SaHinh;
                quyDinh.MoPhong = model.MoPhong;
                quyDinh.DuongTruong = model.DuongTruong;
                quyDinh.GhiChu = model.GhiChu;
            }

            await _context.SaveChangesAsync();

            // quay lại trang QuyDinh luôn để thấy dữ liệu vừa lưu
            return RedirectToAction(nameof(QuyDinh), new { hangId });
        }

        /* =====================================================
         * 5. XÓA HẠNG (CÓ KIỂM TRA RÀNG BUỘC)
         * ===================================================== */
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hang = await _context.Hangs
                .Include(h => h.KhoaHocs)
                .FirstOrDefaultAsync(h => h.IdHang == id);

            if (hang == null) return NotFound();

            if (hang.KhoaHocs.Any())
            {
                TempData["Error"] = "Không thể xóa hạng đã có khóa học.";
                return RedirectToAction(nameof(Index));
            }

            _context.Hangs.Remove(hang);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
