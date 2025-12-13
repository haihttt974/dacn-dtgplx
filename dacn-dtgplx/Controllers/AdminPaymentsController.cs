using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class AdminPaymentsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminPaymentsController(DtGplxContext context)
        {
            _context = context;
        }

        // ===================================================
        // INDEX – Danh sách thanh toán
        // ===================================================
        public async Task<IActionResult> Index()
        {
            var data = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(dk => dk.HoSo)
                        .ThenInclude(hs => hs.User)
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(dk => dk.KhoaHoc)
                .Select(h => new AdminPaymentViewModel
                {
                    IdThanhToan = h.IdThanhToan,

                    TenHocVien = h.IdDangKyNavigation!.HoSo.User.TenDayDu,
                    Email = h.IdDangKyNavigation.HoSo.User.Email,
                    SoDienThoai = h.IdDangKyNavigation.HoSo.User.SoDienThoai,

                    TenKhoaHoc = h.IdDangKyNavigation.KhoaHoc.TenKhoaHoc,

                    SoTien = h.SoTien,
                    PhuongThucThanhToan = h.PhuongThucThanhToan,
                    NgayThanhToan = h.NgayThanhToan.HasValue
                        ? DateOnly.FromDateTime(h.NgayThanhToan.Value)
                        : (DateOnly?)null,
                    TrangThai = h.TrangThai
                })
                .OrderByDescending(x => x.IdThanhToan)
                .ToListAsync();

            return View(data);
        }

        // ===================================================
        // XÁC NHẬN THANH TOÁN
        // ===================================================
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var hd = await _context.HoaDonThanhToans.FindAsync(id);

            if (hd == null)
                return NotFound();

            hd.TrangThai = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
