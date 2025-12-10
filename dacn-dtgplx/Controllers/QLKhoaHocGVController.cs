using dacn_dtgplx.Models;
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


}
