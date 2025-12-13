using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public class QLKhoaHocGVController : Controller
{
    private readonly DtGplxContext _context;

    public QLKhoaHocGVController(DtGplxContext context)
    {
        _context = context;
    }

    // =====================================================
    // 1. INDEX - Danh sách khóa học của GV
    // =====================================================
    public async Task<IActionResult> Index(string search)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Login", "Auth");

        var gv = await _context.TtGiaoViens
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (gv == null)
            return NotFound("Không tìm thấy giáo viên");

        ViewBag.IdGiaoVien = gv.TtGiaoVienId;

        // Parse JSON lichDay
        List<int> khoaHocIds;

        if (gv.LichDay.Trim().StartsWith("["))
        {
            khoaHocIds = JsonConvert.DeserializeObject<List<int>>(gv.LichDay);
        }
        else
        {
            khoaHocIds = new List<int> { JsonConvert.DeserializeObject<int>(gv.LichDay) };
        }

        var query = _context.KhoaHocs
            .Where(k => khoaHocIds.Contains(k.KhoaHocId))
            .AsQueryable();

        // ==========================
        // TÌM KIẾM
        // ==========================
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(k =>
                k.KhoaHocId.ToString().Contains(search) ||
                k.TenKhoaHoc.Contains(search)
            );
        }

        ViewBag.Search = search;

        var khoaHocs = await query.ToListAsync();

        return View(khoaHocs);
        
}


    // =====================================================
    // 2. STUDENT (Hồ sơ học viên của khóa học)
    // =====================================================

    public async Task<IActionResult> Student(int idKhoaHoc, string? search = "")
    {
        // Lấy danh sách học viên theo khóa học
        var query = _context.DangKyHocs
            .Where(x => x.KhoaHocId == idKhoaHoc)
            .Include(x => x.HoSo)
                .ThenInclude(h => h.User)
            .AsQueryable();

        // Nếu có từ khóa tìm kiếm
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x =>
                x.IdDangKy.ToString().Contains(search) ||
                (!string.IsNullOrEmpty(x.HoSo.User.TenDayDu) && x.HoSo.User.TenDayDu.Contains(search)) ||
                (!string.IsNullOrEmpty(x.HoSo.User.SoDienThoai) && x.HoSo.User.SoDienThoai.Contains(search)) ||
                (!string.IsNullOrEmpty(x.HoSo.User.Email) && x.HoSo.User.Email.Contains(search))
            );
        }

        var danhSachHS = await query.ToListAsync();

        // Lấy thông tin khóa học
        var khoaHoc = await _context.KhoaHocs
            .FirstOrDefaultAsync(k => k.KhoaHocId == idKhoaHoc);

        ViewBag.TenKhoaHoc = khoaHoc?.TenKhoaHoc;
        ViewBag.Search = search;

        return View(danhSachHS);
    }



    // =====================================================
    // 3. Lấy chi tiết Hồ sơ 
    // =====================================================
    public async Task<IActionResult> StudentDetail(int idDangKy)
    {
        var dk = await _context.DangKyHocs
            .Include(x => x.HoSo)
                .ThenInclude(h => h.User)   
            .FirstOrDefaultAsync(x => x.IdDangKy == idDangKy);

        if (dk == null)
            return NotFound();

        return PartialView("_StudentDetail", dk.HoSo.User);
    }
    // =====================================================
    // Lịch dạy 
    // =====================================================
    public async Task<IActionResult> LichDay(
        int id,
        string mode = "day",
        string? selectedDate = null,
        int? selectedWeek = null,
        int? selectedMonth = null,
        int? selectedYear = null)
    {
        var giaoVien = await _context.TtGiaoViens
            .FirstOrDefaultAsync(g => g.TtGiaoVienId == id);

        if (giaoVien == null) return NotFound();

        // =====================================================
        // LẤY DANH SÁCH KHÓA HỌC GIÁO VIÊN PHỤ TRÁCH
        // =====================================================
        List<int> khoaHocIds = new();

        if (!string.IsNullOrEmpty(giaoVien.LichDay))
        {
            if (giaoVien.LichDay.Trim().StartsWith("["))
                khoaHocIds = JsonConvert.DeserializeObject<List<int>>(giaoVien.LichDay);
            else
                khoaHocIds = new List<int> { JsonConvert.DeserializeObject<int>(giaoVien.LichDay) };
        }

        var lichList = await _context.LichHocs
            .Where(l => khoaHocIds.Contains(l.KhoaHocId))
            .ToListAsync();

        // =====================================================
        // XÁC ĐỊNH NGÀY ĐANG XEM (filterDate)
        // =====================================================
        DateTime filterDate = DateTime.Today;

        // Nếu mode = day → nhận selectedDate
        if (!string.IsNullOrEmpty(selectedDate))
            DateTime.TryParse(selectedDate, out filterDate);

        // Nếu mode = month → tạo ngày đầu tháng
        if (mode == "month")
        {
            int m = selectedMonth ?? filterDate.Month;
            int y = selectedYear ?? filterDate.Year;

            filterDate = new DateTime(y, m, 1);
        }

        // =====================================================
        // TẠO LỊCH DAY ITEMS
        // =====================================================
        var lichDayItems = new List<LichDayItem>();

        foreach (var lich in lichList)
        {
            var khoaHoc = await _context.KhoaHocs.FindAsync(lich.KhoaHocId);

            lichDayItems.Add(new LichDayItem
            {
                KhoaHocId = lich.KhoaHocId,
                TenKhoaHoc = khoaHoc?.TenKhoaHoc ?? "(Không tìm thấy)",
                NgayHoc = lich.NgayHoc.ToDateTime(TimeOnly.MinValue),
                GioHoc = $"{lich.TgBatDau:hh\\:mm} - {lich.TgKetThuc:hh\\:mm}",
                DiaDiem = lich.DiaDiem,
                NoiDung = lich.NoiDung
            });
        }

        IEnumerable<LichDayItem> filtered = lichDayItems;

        // =====================================================
        // FILTER DỮ LIỆU THEO MODE
        // =====================================================
        switch (mode.ToLower())
        {
            case "day":
                filtered = filtered.Where(x => x.NgayHoc.Date == filterDate.Date);
                break;

            case "month":
                filtered = filtered.Where(x =>
                    x.NgayHoc.Month == filterDate.Month &&
                    x.NgayHoc.Year == filterDate.Year);
                break;

            case "week":
                // xử lý bên dưới
                break;
        }

        // =====================================================
        // TẠO TUẦN (±8 tuần QUANH NGÀY ĐANG XEM)
        // =====================================================
        var weeks = new List<(int Index, DateTime Start, DateTime End)>();

        // Tìm thứ hai đầu tuần chứa filterDate
        var mondayOfCurrentWeek = filterDate.AddDays(-(int)filterDate.DayOfWeek + 1);

        int range = 8; // số tuần trước – sau (tổng 17 tuần)

        for (int i = -range; i <= range; i++)
        {
            var start = mondayOfCurrentWeek.AddDays(i * 7);
            var end = start.AddDays(6);

            weeks.Add((i, start, end));
        }

        // Tuần hiện tại mặc định = 0
        if (mode == "week" && selectedWeek == null)
            selectedWeek = 0;

        // Lọc theo tuần
        if (mode == "week" && selectedWeek != null)
        {
            var w = weeks.First(x => x.Index == selectedWeek);
            filtered = filtered.Where(x =>
                x.NgayHoc.Date >= w.Start.Date &&
                x.NgayHoc.Date <= w.End.Date);
        }

        // =====================================================
        // TẠO VIEW MODEL
        // =====================================================
        var vm = new LichDayViewModel
        {
            GiaoVien = giaoVien,

            Mode = mode,
            CurrentDate = filterDate,

            SelectedWeek = selectedWeek,
            SelectedMonth = filterDate.Month,
            SelectedYear = filterDate.Year,

            Weeks = weeks.Select(w => new WeekItem
            {
                WeekIndex = w.Index,
                Start = w.Start,
                End = w.End
            }).ToList(),

            LichDayItems = filtered.OrderBy(x => x.NgayHoc).ToList()
        };

        return View("LichDay", vm);
    }


}
