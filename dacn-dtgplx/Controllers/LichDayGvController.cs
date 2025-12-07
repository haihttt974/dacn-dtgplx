using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class LichDayGvController : Controller
{
    private readonly DtGplxContext _context;

    public LichDayGvController(DtGplxContext context)
    {
        _context = context;
    }

    // ================================
    // 1. INDEX + AJAX PARTIAL
    // ================================
    public async Task<IActionResult> Index(string search, string hang)
    {
        hang = hang?.Trim();

        // Danh sách hạng
        var hangList = await _context.Hangs
            .Select(h => h.MaHang)
            .OrderBy(h => h)
            .ToListAsync();

        ViewBag.AllHangs = hangList;
        ViewBag.Search = search;
        ViewBag.Hang = hang;

        // Lấy toàn bộ giáo viên
        var giaoViens = await _context.TtGiaoViens
            .Include(g => g.User)
            .ToListAsync();

        var list = new List<GiaoVienIndexVM>();

        foreach (var gv in giaoViens)
        {
            var vm = new GiaoVienIndexVM
            {
                Id = gv.TtGiaoVienId,
                Ten = gv.User.TenDayDu,
                ChuyenMon = gv.ChuyenMon,
                NgayVaoLam = gv.NgayBatDauLam,
                ChuyenDaoTao = new List<string>(),
                LichDay = new List<int>()
            };
            if (!string.IsNullOrEmpty(gv.ChuyenDaoTao))
            {
                try { vm.ChuyenDaoTao = JsonSerializer.Deserialize<List<string>>(gv.ChuyenDaoTao); }
                catch { }
            }
            if (!string.IsNullOrEmpty(gv.LichDay))
            {
                try { vm.LichDay = JsonSerializer.Deserialize<List<int>>(gv.LichDay); }
                catch { }
            }
            list.Add(vm);
        }
        // Lọc theo tên
        if (!string.IsNullOrWhiteSpace(search))
        {
            list = list.Where(g =>
                g.Ten.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        // Mapping hạng chuẩn
        var map = new Dictionary<string, List<string>>
        {
            { "A",  new() { "A" } },
            { "A1", new() { "A1" } },
            { "A2", new() { "A2" } },

            { "B",  new() { "B" } },
            { "B1", new() { "B1" } },
            { "B2", new() { "B2" } },
            { "BE", new() { "BE" } },
            { "C",  new() { "C" } },
            { "C1", new() { "C1" } },
            { "C2", new() { "C2" } },

            { "D",  new() { "D" } },
            { "D1", new() { "D1" } },
            { "DE", new() { "DE" } },

            { "CE", new() { "CE" } },
        };
        // Lọc theo hạng
        if (!string.IsNullOrWhiteSpace(hang) && map.ContainsKey(hang))
        {
            var allowed = map[hang];

            list = list.Where(g =>
                g.ChuyenDaoTao.Any(cdt => allowed.Contains(cdt))
            ).ToList();
        }

        // Nếu là AJAX → trả về Partial
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_Table", list);
        }

        return View(list);
    }

    // ================================
    // 2. SCHEDULE
    // ================================
    public async Task<IActionResult> Schedule(int id)
    {
        var gv = await _context.TtGiaoViens
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.TtGiaoVienId == id);

        if (gv == null) return NotFound();

        // ---- Parse lịch dạy ----
        List<int> lichDay = new();
        if (!string.IsNullOrEmpty(gv.LichDay))
            lichDay = JsonSerializer.Deserialize<List<int>>(gv.LichDay);

        // ---- Parse chuyên đào tạo (Hạng phụ trách của GV) ----
        List<string> hangPhuTrach = new();
        if (!string.IsNullOrEmpty(gv.ChuyenDaoTao))
            hangPhuTrach = JsonSerializer.Deserialize<List<string>>(gv.ChuyenDaoTao);

        // Lấy tất cả GV khác
        var otherGVs = await _context.TtGiaoViens
            .Where(g => g.TtGiaoVienId != id)
            .ToListAsync();

        // Tập khóa học đã có GV phụ trách
        var khoaHocDaCoGV = new HashSet<int>();

        foreach (var g in otherGVs)
        {
            if (!string.IsNullOrEmpty(g.LichDay))
            {
                var ds = JsonSerializer.Deserialize<List<int>>(g.LichDay);
                foreach (var kh in ds)
                    khoaHocDaCoGV.Add(kh);
            }
        }

        // ================================
        // LẤY TẤT CẢ KHÓA HỌC THUỘC HẠNG GV CÓ THỂ DẠY
        // ================================
        var khoaHocAll = await _context.KhoaHocs
            .Include(k => k.IdHangNavigation)
            .Include(k => k.LichHocs)
            .Where(k => k.IsActive == true &&
                        hangPhuTrach.Contains(k.IdHangNavigation.MaHang))
            .ToListAsync();

        khoaHocAll = khoaHocAll
            .Where(k => !lichDay.Contains(k.KhoaHocId))
            .Where(k => !khoaHocDaCoGV.Contains(k.KhoaHocId))
            .ToList();

        // Sau khi có khoaHocAll
        var khoaHocJs = khoaHocAll.Select(k => new
        {
            khoaHocId = k.KhoaHocId,
            tenKhoaHoc = k.TenKhoaHoc,
            thoiGianBatDau = k.NgayBatDau?.ToString("yyyy-MM-dd") ?? "",
            thoiGianKetThuc = k.NgayKetThuc?.ToString("yyyy-MM-dd") ?? "",
            soLuongToiDa = k.SlToiDa,
            maHang = k.IdHangNavigation.MaHang
        }).ToList();

        ViewBag.KhoaHocJs = khoaHocJs;

        // ================================
        // LẤY LỊCH HỌC CỦA GV ĐANG DẠY
        // ================================
        var khoaHocDangDay = await _context.KhoaHocs
            .Where(k => lichDay.Contains(k.KhoaHocId))
            .Include(k => k.LichHocs)
            .Include(k => k.IdHangNavigation)
            .ToListAsync();

        ViewBag.LichHienTai = khoaHocDangDay;

        // ================================
        // KIỂM TRA KHÓA HỌC CÓ TRÙNG GIỜ HAY KHÔNG
        // ================================
        var xungDot = new Dictionary<int, string>();

        foreach (var kh in khoaHocAll)
        {
            bool conflict = false;

            foreach (var khDangDay in khoaHocDangDay)
            {
                foreach (var lh1 in kh.LichHocs)
                {
                    foreach (var lh2 in khDangDay.LichHocs)
                    {
                        if (lh1.NgayHoc == lh2.NgayHoc &&
                           lh1.TgBatDau < lh2.TgKetThuc &&
                           lh2.TgBatDau < lh1.TgKetThuc)
                        {
                            conflict = true;
                            xungDot[kh.KhoaHocId] =
                                $"Khóa {kh.TenKhoaHoc} trùng giờ với {khDangDay.TenKhoaHoc}";
                            break;
                        }
                    }
                    if (conflict) break;
                }
            }
        }
        var lichHocJs = _context.LichHocs
            .GroupBy(l => l.KhoaHocId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(l => new {
                    ngay = l.NgayHoc.ToString("yyyy-MM-dd"),
                    bd = l.TgBatDau.ToString("HH:mm"),
                    kt = l.TgKetThuc.ToString("HH:mm")
                }).ToList()
            );

        ViewBag.LichHocJs = lichHocJs;
        ViewBag.XungDot = xungDot;
        ViewBag.KhoaHocAll = khoaHocAll;
        ViewBag.HangPhuTrach = hangPhuTrach;
        ViewBag.LichDay = lichDay;
        Console.WriteLine(">>> HẠNG GIÁO VIÊN PHỤ TRÁCH: " + string.Join(", ", hangPhuTrach));

        foreach (var kh in khoaHocAll)
        {
            Console.WriteLine($">>> KH: {kh.TenKhoaHoc} — {kh.IdHangNavigation.MaHang}");
        }

        Console.WriteLine(">>> SỐ KHÓA HỌC LOAD ĐƯỢC: " + khoaHocAll.Count);

        return View(gv);
    }

    // ================================
    // 3. SAVE SCHEDULE
    // ================================
    [HttpPost]
    public async Task<IActionResult> SaveSchedule(int id, List<int> khoaHocIds)
    {
        var gv = await _context.TtGiaoViens.FindAsync(id);
        if (gv == null) return NotFound();

        // lấy lịch cũ
        List<int> lichCu = new List<int>();
        if (!string.IsNullOrEmpty(gv.LichDay))
            lichCu = JsonSerializer.Deserialize<List<int>>(gv.LichDay);

        // gộp lại (không trùng)
        var lichMoi = lichCu
            .Union(khoaHocIds)
            .ToList();

        // lưu lại JSON
        gv.LichDay = JsonSerializer.Serialize(lichMoi);

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
