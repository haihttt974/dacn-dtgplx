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
        // INDEX – Danh sách thanh toán (Khóa học + Thuê xe)
        // ===================================================
        public async Task<IActionResult> Index()
        {
            var data = await _context.HoaDonThanhToans
                // Đăng ký học
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(dk => dk.HoSo)
                        .ThenInclude(hs => hs.User)
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(dk => dk.KhoaHoc)

                // Phiếu thuê xe
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.User)
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)

                .Select(h => new AdminPaymentViewModel
                {
                    IdThanhToan = h.IdThanhToan,

                    // ========================
                    // XÁC ĐỊNH NGƯỜI THANH TOÁN
                    // ========================
                    TenNguoiThanhToan =
                        h.IdDangKyNavigation != null
                            ? h.IdDangKyNavigation.HoSo.User.TenDayDu
                            : h.PhieuTx!.User.TenDayDu,

                    Email =
                        h.IdDangKyNavigation != null
                            ? h.IdDangKyNavigation.HoSo.User.Email
                            : h.PhieuTx!.User.Email,

                    SoDienThoai =
                        h.IdDangKyNavigation != null
                            ? h.IdDangKyNavigation.HoSo.User.SoDienThoai
                            : h.PhieuTx!.User.SoDienThoai,

                    // ========================
                    // LOẠI THANH TOÁN
                    // ========================
                    LoaiThanhToan =
                        h.IdDangKyNavigation != null
                            ? "Khóa học"
                            : "Thuê xe",

                    // ========================
                    // CHI TIẾT
                    // ========================
                    TenKhoaHoc =
                        h.IdDangKyNavigation != null
                            ? h.IdDangKyNavigation.KhoaHoc.TenKhoaHoc
                            : null,

                    XeTapLai =
                        h.PhieuTx != null
                            ? h.PhieuTx.Xe.LoaiXe
                            : null,

                    SoTien = h.SoTien,
                    PhuongThucThanhToan = h.PhuongThucThanhToan,
                    NgayThanhToan = h.NgayThanhToan.HasValue
                         ? DateOnly.FromDateTime(h.NgayThanhToan.Value)
                         : null,
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
            hd.TrangThai = true;
            hd.NgayThanhToan = DateTime.Now; 
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
