using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers.Admin
{
    public class AdminProfilesController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminProfilesController(DtGplxContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. DANH SÁCH HỒ SƠ
        // status = 0 → Chưa duyệt
        // status = 1 → Đã duyệt
        // status = 2 → Từ chối
        // =====================================================
        public async Task<IActionResult> Index(string search, int? status)
        {
            var query = _context.HoSoThiSinhs
                .Include(h => h.User)
                .Include(h => h.DangKyHocs)
                    .ThenInclude(d => d.KhoaHoc)
                .AsQueryable();

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case 0:
                        query = query.Where(h => h.DaDuyet == null);
                        break;
                    case 1:
                        query = query.Where(h => h.DaDuyet == true);
                        break;
                    case 2:
                        query = query.Where(h => h.DaDuyet == false);
                        break;
                }
            }

            // Tìm kiếm theo tên / SDT / CCCD
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h =>
                    h.User.TenDayDu.Contains(search) ||
                    h.User.SoDienThoai.Contains(search) ||
                    h.User.Cccd.Contains(search)
                );
            }

            var list = await query
                .OrderByDescending(h => h.NgayDk)
                .ToListAsync();

            return View(list);
        }

        // =====================================================
        // 2. CHI TIẾT HỒ SƠ
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            var hoSo = await _context.HoSoThiSinhs
                .Include(h => h.User)
                .Include(h => h.DangKyHocs)
                    .ThenInclude(d => d.KhoaHoc)
                .Include(h => h.KetQuaHocTaps)
                .FirstOrDefaultAsync(h => h.HoSoId == id);

            if (hoSo == null)
                return NotFound();

            return View(hoSo);
        }

        // =====================================================
        // 3. DUYỆT HỒ SƠ (DaDuyet = true)
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var hoSo = await _context.HoSoThiSinhs.FindAsync(id);
            if (hoSo == null)
                return NotFound();

            hoSo.DaDuyet = true;
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã duyệt hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // 4. TỪ CHỐI HỒ SƠ (DaDuyet = false)
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? note)
        {
            var hoSo = await _context.HoSoThiSinhs.FindAsync(id);
            if (hoSo == null)
                return NotFound();

            hoSo.DaDuyet = false;
            hoSo.GhiChu = note;

            await _context.SaveChangesAsync();

            TempData["error"] = "Đã từ chối hồ sơ!";
            return RedirectToAction(nameof(Index));
        }
    }
}
