using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace dacn_dtgplx.Controllers.Admin
{
    [Route("admin/report")]
    public class AdminReportController : Controller
    {
        private readonly DtGplxContext _context;
        public AdminReportController(DtGplxContext context) => _context = context;

        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            var from = tuNgay ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var to = denNgay ?? DateTime.Now;

            var vm = new AdminReportIndexVM
            {
                TuNgay = from,
                DenNgay = to
            };

            // ================= QUERY GỐC =================
            var hoaDonQuery = _context.HoaDonThanhToans
                .AsNoTracking()
                .Include(x => x.IdDangKyNavigation)
                    .ThenInclude(d => d.KhoaHoc)
                        .ThenInclude(k => k.IdHangNavigation)
                .Where(x =>
                    x.TrangThai == true &&
                    x.NgayThanhToan.HasValue &&
                    x.NgayThanhToan.Value >= from &&
                    x.NgayThanhToan.Value <= to
                );

            // ================= KPI =================
            vm.TongDoanhThu = await hoaDonQuery.SumAsync(x => x.SoTien ?? 0);
            vm.TongHoaDon = await hoaDonQuery.CountAsync();
            vm.TongHocVien = await _context.DangKyHocs.Select(x => x.HoSoId).Distinct().CountAsync();
            vm.TongKhoaHoc = await _context.KhoaHocs.CountAsync(x => x.IsActive == true);

            // ================= DOANH THU NGÀY =================
            var ngayRaw = await hoaDonQuery
                .GroupBy(x => x.NgayThanhToan!.Value.Date)
                .Select(g => new { Ngay = g.Key, Tong = g.Sum(x => x.SoTien ?? 0) })
                .OrderBy(x => x.Ngay)
                .ToListAsync();

            vm.DoanhThuNgay = ngayRaw
                .Select(x => new TimeValueVM
                {
                    Label = x.Ngay.ToString("dd/MM"),
                    Value = x.Tong
                }).ToList();

            // ================= DOANH THU TUẦN =================
            var list = await hoaDonQuery
                .Select(x => new { x.NgayThanhToan!.Value, Tien = x.SoTien ?? 0 })
                .ToListAsync();

            vm.DoanhThuTuan = list
                .GroupBy(x => ISOWeek.GetWeekOfYear(x.Value))
                .Select(g => new TimeValueVM
                {
                    Label = $"Tuần {g.Key}",
                    Value = g.Sum(x => x.Tien)
                })
                .OrderBy(x => x.Label)
                .ToList();

            // ================= DOANH THU THÁNG =================
            var thangRaw = await hoaDonQuery
                .GroupBy(x => new { x.NgayThanhToan!.Value.Year, x.NgayThanhToan.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Tong = g.Sum(x => x.SoTien ?? 0) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            vm.DoanhThuThang = thangRaw
                .Select(x => new TimeValueVM
                {
                    Label = $"{x.Month:D2}/{x.Year}",
                    Value = x.Tong
                }).ToList();

            // ================= DOANH THU QUÝ =================
            var quyRaw = await hoaDonQuery
                .GroupBy(x => new
                {
                    x.NgayThanhToan!.Value.Year,
                    Quy = (x.NgayThanhToan.Value.Month - 1) / 3 + 1
                })
                .Select(g => new { g.Key.Year, g.Key.Quy, Tong = g.Sum(x => x.SoTien ?? 0) })
                .OrderBy(x => x.Year).ThenBy(x => x.Quy)
                .ToListAsync();

            vm.DoanhThuQuy = quyRaw
                .Select(x => new TimeValueVM
                {
                    Label = $"Q{x.Quy}/{x.Year}",
                    Value = x.Tong
                }).ToList();

            // ================= DOANH THU THEO HẠNG =================
            vm.DoanhThuTheoHang = await hoaDonQuery
                .GroupBy(x => x.IdDangKyNavigation.KhoaHoc.IdHangNavigation.MaHang)
                .Select(g => new HangValueVM
                {
                    Ten = g.Key.Trim(),
                    Value = g.Sum(x => x.SoTien ?? 0)
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            // ================= HỌC VIÊN THEO HẠNG =================
            vm.HocVienTheoHang = await _context.DangKyHocs
                .GroupBy(x => x.KhoaHoc.IdHangNavigation.MaHang)
                .Select(g => new HangValueVM
                {
                    Ten = g.Key.Trim(),
                    Value = g.Select(x => x.HoSoId).Distinct().Count()
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            return View(vm);
        }
    }
}
