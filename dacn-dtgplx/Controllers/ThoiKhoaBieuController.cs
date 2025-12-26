using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ThoiKhoaBieuController : Controller
{
    private readonly DtGplxContext _context;

    public ThoiKhoaBieuController(DtGplxContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? week)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        // Xác định tuần
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateOnly currentWeekStart = today.AddDays(-diff);

        DateOnly weekStart = week ?? currentWeekStart;
        DateOnly weekEnd = weekStart.AddDays(6);

        var vm = new ScheduleWeekVm
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            IsCurrentWeek = weekStart == currentWeekStart
        };

        // =========================================================
        // 1️⃣ LỊCH HỌC KHÓA HỌC (đã thanh toán)
        // =========================================================
        var lichHoc = await _context.DangKyHocs
            .Where(dk => dk.HoSo.UserId == userId)
            .Where(dk => _context.HoaDonThanhToans
                .Any(hd => hd.IdDangKy == dk.IdDangKy && hd.TrangThai == true))
            .SelectMany(dk => dk.KhoaHoc.LichHocs)
            .Where(lh => lh.NgayHoc >= weekStart && lh.NgayHoc <= weekEnd)
            .Include(lh => lh.KhoaHoc)
            .Include(lh => lh.LopHoc)
            .Include(lh => lh.XeTapLai)
            .ToListAsync();

        foreach (var lh in lichHoc)
        {
            var isThucHanh = !string.IsNullOrWhiteSpace(lh.NoiDung)
                && lh.NoiDung.Contains("thực hành", StringComparison.OrdinalIgnoreCase);

            var lopText = isThucHanh
                ? "Thực hành"
                : (lh.LopHoc?.TenLop ?? "");

            vm.Items.Add(new ScheduleItemVm
            {
                Type = "KHOA_HOC",
                Ngay = lh.NgayHoc,
                GioBatDau = ConvertTime(lh.TgBatDau),
                GioKetThuc = ConvertTime(lh.TgKetThuc),
                TieuDe = lh.KhoaHoc.TenKhoaHoc ?? "Khóa học",
                NoiDung = lh.NoiDung,
                DiaDiem = lh.DiaDiem,
                Mau = "#4CAF50",
                HoverHtml = $"""
            <b>{lh.KhoaHoc.TenKhoaHoc}</b><br/>
            Ngày: {lh.NgayHoc:dd/MM/yyyy}<br/>
            Giờ: {lh.TgBatDau} - {lh.TgKetThuc}<br/>
            Địa điểm: {lh.DiaDiem}<br/>
            Lớp: {lopText}
        """
            });
        }

        // =========================================================
        // 2️⃣ LỊCH THUÊ XE (đã thanh toán)
        // =========================================================
        var phieuThue = await _context.PhieuThueXe
            .Where(px => px.UserId == userId)
            .Where(px => _context.HoaDonThanhToans
                .Any(hd => hd.PhieuTxId == px.PhieuTxId && hd.TrangThai == true))
            .Include(px => px.Xe)
            .ToListAsync();

        foreach (var px in phieuThue)
        {
            if (px.TgBatDau == null || px.TgThue == null)
                continue;

            DateTime start = px.TgBatDau.Value;
            DateTime end = start.AddHours(px.TgThue.Value);

            var ngay = DateOnly.FromDateTime(start);
            if (ngay < weekStart || ngay > weekEnd)
                continue;

            vm.Items.Add(new ScheduleItemVm
            {
                Type = "THUE_XE",
                Ngay = ngay,
                GioBatDau = start.Hour + start.Minute / 60.0,
                GioKetThuc = end.Hour + end.Minute / 60.0,
                TieuDe = $"Thuê xe {px.Xe.LoaiXe}",
                NoiDung = $"Biển số: {px.Xe.BienSo}",
                DiaDiem = "Sân tập",
                Mau = "#2196F3",
                HoverHtml = $"""
                    <b>Thuê xe tập lái</b><br/>
                    Xe: {px.Xe.LoaiXe}<br/>
                    Biển số: {px.Xe.BienSo}<br/>
                    Giờ: {start:HH:mm} - {end:HH:mm}
                """
            });
        }
        vm.TotalSessions = vm.Items.Count;
        return View(vm);
    }

    // ======================
    // Helper
    // ======================
    private static double ConvertTime(TimeOnly time)
        => time.Hour + time.Minute / 60.0;
}
