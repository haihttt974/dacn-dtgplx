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
    public async Task<IActionResult> Index()
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
            // dạng JSON array: [5,6,7]
            khoaHocIds = JsonConvert.DeserializeObject<List<int>>(gv.LichDay);
        }
        else
        {
            // dạng một số: 5
            khoaHocIds = new List<int> { JsonConvert.DeserializeObject<int>(gv.LichDay) };
        }

        var khoaHocs = await _context.KhoaHocs
                        .Where(k => khoaHocIds.Contains(k.KhoaHocId))
                        .ToListAsync();
        
        return View(khoaHocs);

    }


    // =====================================================
    // 2. STUDENT (Hồ sơ học viên của khóa học)
    // =====================================================
   
    public async Task<IActionResult> Student(int idKhoaHoc)
    {
        var danhSachHS = await _context.DangKyHocs
            .Where(x => x.KhoaHocId == idKhoaHoc)
            .Include(x => x.HoSo)
             .ThenInclude(h => h.User)
            .ToListAsync();

        var khoaHoc = await _context.KhoaHocs
            .FirstOrDefaultAsync(k => k.KhoaHocId == idKhoaHoc);

        ViewBag.TenKhoaHoc = khoaHoc?.TenKhoaHoc;

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
    public async Task<IActionResult> LichDay(int id, string mode = "day", string? selectedDate = null)
    {
        var giaoVien = await _context.TtGiaoViens.FirstOrDefaultAsync(g => g.TtGiaoVienId == id);
        if (giaoVien == null) return NotFound();

        // Lấy danh sách khóa học mà GV đang dạy (LichDay = JSON chứa danh sách id khóa học)
        List<int> khoaHocIds = new();

        if (!string.IsNullOrEmpty(giaoVien.LichDay))
        {
            if (giaoVien.LichDay.Trim().StartsWith("["))
                khoaHocIds = JsonConvert.DeserializeObject<List<int>>(giaoVien.LichDay);
            else
                khoaHocIds = new List<int> { JsonConvert.DeserializeObject<int>(giaoVien.LichDay) };
        }

        // Lấy tất cả lịch học thuộc các khóa GV đang dạy
        var lichList = await _context.LichHocs
            .Where(l => khoaHocIds.Contains(l.KhoaHocId))
            .ToListAsync();

        DateTime today = DateTime.Today;
        DateTime filterDate = today;

        if (!string.IsNullOrEmpty(selectedDate))
        {
            DateTime.TryParse(selectedDate, out filterDate);
        }

        // CHUYỂN LichHoc (DateOnly, TimeOnly) → LichDayItem
        var lichDayItems = new List<LichDayItem>();

        foreach (var lich in lichList)
        {
            var khoaHoc = await _context.KhoaHocs.FindAsync(lich.KhoaHocId);

            lichDayItems.Add(new LichDayItem
            {
                KhoaHocId = lich.KhoaHocId,
                TenKhoaHoc = khoaHoc?.TenKhoaHoc ?? "(Không tìm thấy)",

                // DateOnly → DateTime
                NgayHoc = lich.NgayHoc.ToDateTime(TimeOnly.MinValue),

                // TimeOnly → string
                GioHoc = $"{lich.TgBatDau:hh\\:mm} - {lich.TgKetThuc:hh\\:mm}",

                DiaDiem = lich.DiaDiem,
                NoiDung = lich.NoiDung
            });
        }

        // Áp dụng bộ lọc mode (day/week/month)
        IEnumerable<LichDayItem> filtered = lichDayItems;

        switch (mode.ToLower())
        {
            case "day":
                filtered = filtered.Where(x => x.NgayHoc.Date == filterDate.Date);
                break;

            case "week":
                var startWeek = filterDate.AddDays(-(int)filterDate.DayOfWeek);
                var endWeek = startWeek.AddDays(6);
                filtered = filtered.Where(x => x.NgayHoc.Date >= startWeek.Date &&
                                               x.NgayHoc.Date <= endWeek.Date);
                break;

            case "month":
                filtered = filtered.Where(x => x.NgayHoc.Month == filterDate.Month &&
                                               x.NgayHoc.Year == filterDate.Year);
                break;
        }

        var vm = new LichDayViewModel
        {
            GiaoVien = giaoVien,
            Mode = mode,
            CurrentDate = filterDate,
            LichDayItems = filtered.OrderBy(x => x.NgayHoc).ToList()
        };

        return View("LichDay", vm);
    }

}
