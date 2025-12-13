using System.Security.Claims;
using dacn_dtgplx.DTOs;
using dacn_dtgplx.Helpers;
using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ThiMoPhongController : Controller
{
    private readonly DtGplxContext _context;

    public ThiMoPhongController(DtGplxContext context)
    {
        _context = context;
    }

    // ================================
    // AUTH HELPERS
    // ================================
    private bool IsLoggedIn() => User?.Identity?.IsAuthenticated == true;

    private int? TryGetCurrentUserId()
    {
        // Bạn đang dùng claim "UserId"
        var v = User.FindFirstValue("UserId");

        // fallback nếu hệ thống bạn dùng NameIdentifier
        v ??= User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(v, out var id)) return id;
        return null;
    }

    // ================================
    // HELPERS
    // ================================
    private static List<FlagItem> NormalizeFlags(List<FlagItem> flags)
    {
        return flags
            .Where(f => f != null)
            .GroupBy(f => f.IdThMp)
            .Select(g => g.OrderBy(x => x.TimeSec).First())
            .ToList();
    }

    /// <summary>
    /// DB lưu FRAME (60fps) → GIÂY
    /// </summary>
    private static double FrameToSec(double frame) => frame / 60.0;

    private int TinhDiemTheoThoiDiem(double timePressSec, double startSec, double endSec)
    {
        if (endSec <= startSec) return 0;
        if (timePressSec < startSec || timePressSec > endSec) return 0;

        double duration = endSec - startSec;
        double step = duration / 5.0;

        int index = (int)Math.Floor((timePressSec - startSec) / step);
        index = Math.Clamp(index, 0, 4);

        return 5 - index;
    }

    // ✅ Server đảm bảo LUÔN ĐỦ 10 tình huống: thiếu thì tự thêm flag 0 giây
    private List<FlagItem> BuildFull10Flags(BoDeMoPhong boDe, List<FlagItem> clientFlags)
    {
        var normalized = NormalizeFlags(clientFlags);

        // lấy danh sách 10 tình huống của bộ đề theo thứ tự
        var thIds = boDe.ChiTietBoDeMoPhongs
            .OrderBy(x => x.ThuTu)
            .Select(x => x.IdThMpNavigation)
            .Where(x => x != null)
            .Select(x => x!.IdThMp)
            .Take(10)
            .ToList();

        var map = normalized.ToDictionary(x => x.IdThMp, x => x);

        var full = new List<FlagItem>();
        for (int i = 0; i < thIds.Count; i++)
        {
            var idTh = thIds[i];

            if (map.TryGetValue(idTh, out var f))
            {
                // giữ timeSec client gửi lên
                full.Add(new FlagItem
                {
                    IdThMp = idTh,
                    TimeSec = f.TimeSec
                });
            }
            else
            {
                // ✅ thiếu thì mặc định 0s
                full.Add(new FlagItem
                {
                    IdThMp = idTh,
                    TimeSec = 0
                });
            }
        }

        return full;
    }

    private int TinhTongDiemTuBoDe(BoDeMoPhong boDe, List<FlagItem> flagsFull)
    {
        int tong = 0;

        var mapTh = boDe.ChiTietBoDeMoPhongs
            .Select(x => x.IdThMpNavigation)
            .Where(x => x != null)
            .ToDictionary(x => x!.IdThMp, x => x!);

        foreach (var flag in flagsFull)
        {
            if (!mapTh.TryGetValue(flag.IdThMp, out var th)) continue;

            double startSec = FrameToSec(th.TgBatDau ?? 0);
            double endSec = FrameToSec(th.TgKetThuc ?? 0);

            tong += TinhDiemTheoThoiDiem(
                flag.TimeSec,   // video.currentTime (ABSOLUTE)
                startSec,
                endSec
            );
        }

        return tong;
    }

    // ================================
    // DANH SÁCH BỘ ĐỀ
    // ================================
    public async Task<IActionResult> DanhSachBoDe()
    {
        var userId = TryGetCurrentUserId();

        // 1) Danh sách bộ đề active
        var dsBoDe = await _context.BoDeMoPhongs
            .Where(b => b.IsActive == true)
            .OrderBy(b => b.IdBoDeMoPhong)
            .Select(b => new BoDeMoPhongViewModel
            {
                IdBoDe = b.IdBoDeMoPhong,
                TenBoDe = b.TenBoDe,
                SoTinhHuong = b.SoTinhHuong ?? 10,

                // mặc định (guest hoặc chưa có bài)
                HasResult = false,
                TongDiem = 0,
                KetQua = false,
                SoTinhHuongSai = 0,
                IdBaiLamMoiNhat = null
            })
            .ToListAsync();

        // 2) Nếu chưa login -> trả thẳng view (View sẽ hiện nút login)
        if (userId == null)
            return View(dsBoDe);

        // 3) Lấy tất cả bài làm của user + include đủ để tính sai theo [start,end]
        //    (cần IdThMpNavigation để lấy TgBatDau/TgKetThuc)
        var allAttempts = await _context.BaiLamMoPhongs
            .Where(x => x.UserId == userId.Value)
            .Include(x => x.DiemTungTinhHuongs)
                .ThenInclude(d => d.IdThMpNavigation)
            .ToListAsync();

        // 4) Lấy bài làm mới nhất cho mỗi bộ đề
        var latestDict = allAttempts
            .GroupBy(x => x.IdBoDeMoPhong)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.IdBaiLamTongDiem).First()
            );

        // 5) Map kết quả vào dsBoDe
        foreach (var boDeVm in dsBoDe)
        {
            if (!latestDict.TryGetValue(boDeVm.IdBoDe, out var baiLam)) continue;

            boDeVm.HasResult = true;
            boDeVm.TongDiem = baiLam.TongDiem ?? 0;
            boDeVm.KetQua = baiLam.KetQua ?? false;
            boDeVm.IdBaiLamMoiNhat = baiLam.IdBaiLamTongDiem;

            // ✅ Sai = bấm ngoài khoảng [startSec, endSec]
            int soSai = 0;

            foreach (var d in baiLam.DiemTungTinhHuongs)
            {
                var th = d.IdThMpNavigation;

                // thiếu navigation coi như sai (an toàn)
                if (th == null)
                {
                    soSai++;
                    continue;
                }

                double startSec = FrameToSec(th.TgBatDau ?? 0);
                double endSec = FrameToSec(th.TgKetThuc ?? 0);
                double t = d.ThoiDiemNguoiDungNhan;

                if (t < startSec || t > endSec)
                    soSai++;
            }

            boDeVm.SoTinhHuongSai = soSai;
        }

        return View(dsBoDe);
    }

    // ================================
    // LỊCH SỬ BÀI LÀM (CHỈ XEM)
    // ================================
    public async Task<IActionResult> LichSuBaiLam(int idBaiLam)
    {
        var baiLam = await _context.BaiLamMoPhongs
            .Include(b => b.DiemTungTinhHuongs)
                .ThenInclude(d => d.IdThMpNavigation)
            .Include(b => b.IdBoDeMoPhongNavigation)
                .ThenInclude(bd => bd.ChiTietBoDeMoPhongs)
                    .ThenInclude(ct => ct.IdThMpNavigation)
            .FirstOrDefaultAsync(b => b.IdBaiLamTongDiem == idBaiLam);

        if (baiLam == null)
            return NotFound();

        var boDe = baiLam.IdBoDeMoPhongNavigation;

        var vm = new LichSuMoPhongViewModel
        {
            IdBoDe = boDe.IdBoDeMoPhong,
            IdBaiLam = baiLam.IdBaiLamTongDiem,
            TongDiem = baiLam.TongDiem ?? 0,
            KetQua = baiLam.KetQua ?? false
        };

        // 1️⃣ Load 10 tình huống (giống LamBai)
        foreach (var ct in boDe.ChiTietBoDeMoPhongs.OrderBy(x => x.ThuTu))
        {
            var th = ct.IdThMpNavigation;
            if (th == null) continue;

            double startSec = (th.TgBatDau ?? 0) / 60.0;
            double endSec = (th.TgKetThuc ?? 0) / 60.0;

            vm.TinhHuongs.Add(new TinhHuongItem2
            {
                IdThMp = th.IdThMp,
                TieuDe = th.TieuDe ?? "",
                VideoUrl = NormalizeStaticPath(th.VideoUrl),
                ScoreStartSec = startSec,
                ScoreEndSec = endSec,
                HintImageUrl = NormalizeStaticPath(th.UrlAnhMeo)
            });
        }

        // 2️⃣ Load flags đã bấm
        vm.Flags = baiLam.DiemTungTinhHuongs
            .Select(d => new ReviewFlagItem
            {
                IdThMp = d.IdThMp,
                TimeSec = d.ThoiDiemNguoiDungNhan
            })
            .ToList();


        return View(vm);
    }

    // ================================
    // LÀM BÀI THI
    // ================================
    public async Task<IActionResult> LamBai(int idBoDe)
    {
        var boDe = await _context.BoDeMoPhongs
            .Include(b => b.ChiTietBoDeMoPhongs)
                .ThenInclude(ct => ct.IdThMpNavigation)
            .FirstOrDefaultAsync(b => b.IdBoDeMoPhong == idBoDe);

        if (boDe == null) return NotFound();

        var vm = new ThiTrialViewModel
        {
            IdBoDe = boDe.IdBoDeMoPhong
        };

        foreach (var ct in boDe.ChiTietBoDeMoPhongs.OrderBy(x => x.ThuTu))
        {
            var th = ct.IdThMpNavigation;
            if (th == null) continue;

            double startSec = FrameToSec(th.TgBatDau ?? 0);
            double endSec = FrameToSec(th.TgKetThuc ?? 0);

            var item = new TinhHuongItem2
            {
                IdThMp = th.IdThMp,
                TieuDe = th.TieuDe ?? "",
                VideoUrl = NormalizeStaticPath(th.VideoUrl),

                // ✅ ABSOLUTE TIME
                ScoreStartSec = startSec,
                ScoreEndSec = endSec,

                HintImageUrl = NormalizeStaticPath(th.UrlAnhMeo)
            };

            // (tuỳ bạn có dùng item.Mocs không)
            double step = (endSec - startSec) / 5.0;
            item.Mocs.Clear();
            for (int i = 0; i < 5; i++)
            {
                item.Mocs.Add(new MocDiemItem
                {
                    Diem = 5 - i,
                    TimeSec = startSec + step * i
                });
            }

            vm.TinhHuongs.Add(item);
        }

        return View(vm);
    }

    // ================================
    // LƯU KẾT QUẢ (Guest: không lưu DB, User: lưu DB)
    // ================================
    [HttpPost]
    public async Task<IActionResult> LuuKetQua([FromBody] KetQuaRequest request)
    {
        if (request == null) return BadRequest("Request null.");
        if (request.IdBoDe <= 0) return BadRequest("IdBoDe không hợp lệ.");
        if (request.Flags == null) request.Flags = new List<FlagItem>(); // cho phép rỗng, server tự fill 0

        var boDe = await _context.BoDeMoPhongs
            .Include(b => b.ChiTietBoDeMoPhongs)
                .ThenInclude(ct => ct.IdThMpNavigation)
            .FirstOrDefaultAsync(b => b.IdBoDeMoPhong == request.IdBoDe);

        if (boDe == null) return NotFound();

        // ✅ đảm bảo đủ 10 tình huống (thiếu thì tự thêm 0s)
        var flagsFull10 = BuildFull10Flags(boDe, request.Flags);

        int tongDiem = TinhTongDiemTuBoDe(boDe, flagsFull10);
        bool dat = tongDiem >= 35;

        // ================================
        // GUEST: KHÔNG LƯU DB
        // ================================
        if (!IsLoggedIn())
        {
            return Ok(new
            {
                success = true,
                tongDiem,
                dat,
                isGuest = true
            });
        }

        // ================================
        // LOGIN: LƯU DB
        // ================================
        var userId = TryGetCurrentUserId();
        if (userId == null)
            return Unauthorized("Không lấy được UserId từ Claims.");

        var baiLam = new BaiLamMoPhong
        {
            UserId = userId.Value,
            IdBoDeMoPhong = boDe.IdBoDeMoPhong,
            TongDiem = tongDiem,
            KetQua = dat
        };

        _context.BaiLamMoPhongs.Add(baiLam);
        await _context.SaveChangesAsync();

        // lưu chi tiết 10 tình huống
        foreach (var f in flagsFull10)
        {
            _context.DiemTungTinhHuongs.Add(new DiemTungTinhHuong
            {
                IdBaiLamTongDiem = baiLam.IdBaiLamTongDiem,
                IdThMp = f.IdThMp,
                ThoiDiemNguoiDungNhan = f.TimeSec
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            tongDiem,
            dat,
            isGuest = false,
            idBaiLam = baiLam.IdBaiLamTongDiem
        });
    }

    // ================================
    // KẾT QUẢ
    // ================================
    public async Task<IActionResult> KetQua(int id)
    {
        var baiLam = await _context.BaiLamMoPhongs
            .Include(b => b.DiemTungTinhHuongs)
                .ThenInclude(d => d.IdThMpNavigation)
            .FirstOrDefaultAsync(b => b.IdBaiLamTongDiem == id);

        if (baiLam == null) return NotFound();

        var vm = new KetQuaThiViewModel
        {
            TongDiem = baiLam.TongDiem ?? 0,
            KetQua = baiLam.KetQua ?? false
        };

        foreach (var d in baiLam.DiemTungTinhHuongs)
        {
            var th = d.IdThMpNavigation;
            if (th == null) continue;

            double startSec = FrameToSec(th.TgBatDau ?? 0);
            double endSec = FrameToSec(th.TgKetThuc ?? 0);

            int diem = TinhDiemTheoThoiDiem(
                d.ThoiDiemNguoiDungNhan,
                startSec,
                endSec
            );

            vm.ChiTiet.Add(new ChiTietKetQuaItem
            {
                TieuDe = th.TieuDe ?? "",
                ThoiDiemNhan = d.ThoiDiemNguoiDungNhan,
                Diem = diem
            });
        }

        return View(vm);
    }

    private string NormalizeStaticPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";

        if (path.StartsWith("wwwroot"))
            path = path.Substring("wwwroot".Length);

        if (!path.StartsWith("/"))
            path = "/" + path;

        return path.Replace("\\", "/");
    }

    // ================================
    // LÀM BÀI NGẪU NHIÊN (10 tình huống)
    // - Tỷ lệ chương: 2-1-2-1-2-2 (tổng 10) theo bảng % bạn gửi
    // - Có 1 hoặc 2 câu khó (Kho = true)
    // - Dùng lại View LamBai
    // ================================
    public async Task<IActionResult> LamBaiNgauNhien()
    {
        // Tỷ lệ cho 10 câu theo %: 20-10-20-10-20-20 => 2-1-2-1-2-2
        // Nếu IdChuongMp của bạn không phải 1..6 theo thứ tự, thì đổi sang map theo ThuTu/OrderBy
        var quotaByChuong = new Dictionary<int, int>
        {
            { 1, 2 },
            { 2, 1 },
            { 3, 2 },
            { 4, 1 },
            { 5, 2 },
            { 6, 2 }
        };

        var rng = new Random();

        // Load 6 chương + danh sách tình huống
        var chuongs = await _context.ChuongMoPhongs
            .Include(c => c.TinhHuongMoPhongs)
            .OrderBy(c => c.ThuTu)
            .ToListAsync();

        // Lấy hardCount = 1 hoặc 2
        int hardCount = rng.Next(1, 3);

        // Gom ứng viên theo chương
        // Lưu ý: bạn đang có property th.Kho (bool?) ở TinhHuongMoPhong
        // Và TgBatDau/TgKetThuc frame -> giây
        var picked = new List<TinhHuongMoPhong>();

        // 1) Pick câu khó trước (1-2 câu) từ toàn bộ, ưu tiên rải đều theo chương
        var allHard = chuongs
            .SelectMany(c => c.TinhHuongMoPhongs)
            .Where(t => t.Kho == true)
            .OrderBy(_ => rng.Next())
            .ToList();

        // Nếu DB thiếu câu khó thì fallback: lấy tối đa có thể
        hardCount = Math.Min(hardCount, allHard.Count);

        // Chọn hard theo kiểu “ưu tiên chương còn quota”
        foreach (var hard in allHard)
        {
            if (picked.Count(x => x.Kho == true) >= hardCount) break;

            // tìm chương của hard
            var chuongOfHard = chuongs.FirstOrDefault(c => c.TinhHuongMoPhongs.Any(x => x.IdThMp == hard.IdThMp));
            if (chuongOfHard == null) continue;

            int chuongId = chuongOfHard.IdChuongMp;

            // nếu chương này còn quota thì lấy luôn để không phá cơ cấu
            if (quotaByChuong.TryGetValue(chuongId, out var q) && q > 0)
            {
                picked.Add(hard);
                quotaByChuong[chuongId] = q - 1;
            }

            // nếu quota chương đã hết, vẫn có thể lấy hard (nhưng sẽ làm lệch cơ cấu)
            // -> ở đây mình KHÔNG lấy để giữ đúng tỷ lệ.
        }

        // Nếu chưa đủ hardCount (do quota), thì cho phép lấy hard ở chương khác còn quota
        if (picked.Count(x => x.Kho == true) < hardCount)
        {
            foreach (var c in chuongs)
            {
                if (!quotaByChuong.TryGetValue(c.IdChuongMp, out var q) || q <= 0) continue;

                var hardInChuong = c.TinhHuongMoPhongs
                    .Where(t => t.Kho == true && picked.All(p => p.IdThMp != t.IdThMp))
                    .OrderBy(_ => rng.Next())
                    .ToList();

                foreach (var h in hardInChuong)
                {
                    if (picked.Count(x => x.Kho == true) >= hardCount) break;
                    picked.Add(h);
                    quotaByChuong[c.IdChuongMp] = quotaByChuong[c.IdChuongMp] - 1;
                }

                if (picked.Count(x => x.Kho == true) >= hardCount) break;
            }
        }

        // 2) Fill các câu còn lại theo quota từng chương (ưu tiên câu không khó trước)
        foreach (var c in chuongs)
        {
            if (!quotaByChuong.TryGetValue(c.IdChuongMp, out var need) || need <= 0) continue;

            // ưu tiên câu thường trước, rồi mới đến câu khó nếu thiếu
            var normals = c.TinhHuongMoPhongs
                .Where(t => t.Kho != true && picked.All(p => p.IdThMp != t.IdThMp))
                .OrderBy(_ => rng.Next())
                .Take(need)
                .ToList();

            picked.AddRange(normals);
            need -= normals.Count;

            if (need > 0)
            {
                var more = c.TinhHuongMoPhongs
                    .Where(t => picked.All(p => p.IdThMp != t.IdThMp))
                    .OrderBy(_ => rng.Next())
                    .Take(need)
                    .ToList();

                picked.AddRange(more);
            }
        }

        // 3) Nếu vì lý do nào đó vẫn thiếu (DB thiếu câu ở chương),
        //    fill random từ tất cả tình huống còn lại để đủ 10
        if (picked.Count < 10)
        {
            var pool = chuongs.SelectMany(c => c.TinhHuongMoPhongs)
                .Where(t => picked.All(p => p.IdThMp != t.IdThMp))
                .OrderBy(_ => rng.Next())
                .Take(10 - picked.Count)
                .ToList();

            picked.AddRange(pool);
        }

        // 4) Shuffle lại thứ tự 10 tình huống (đề random)
        picked = picked.OrderBy(_ => rng.Next()).Take(10).ToList();

        // Build VM giống LamBai
        var vm = new ThiTrialViewModel
        {
            IdBoDe = 0 // ✅ 0 = đề ngẫu nhiên (để JS biết submit sang endpoint khác)
        };

        foreach (var th in picked)
        {
            double startSec = FrameToSec(th.TgBatDau ?? 0);
            double endSec = FrameToSec(th.TgKetThuc ?? 0);

            vm.TinhHuongs.Add(new TinhHuongItem2
            {
                IdThMp = th.IdThMp,
                TieuDe = th.TieuDe ?? "",
                VideoUrl = NormalizeStaticPath(th.VideoUrl),
                ScoreStartSec = startSec,
                ScoreEndSec = endSec,
                HintImageUrl = NormalizeStaticPath(th.UrlAnhMeo),
                Kho = th.Kho ?? false
            });
        }

        return View("LamBai", vm);
    }

    // ================================
    // CHẤM ĐIỂM ĐỀ NGẪU NHIÊN
    // - Không lưu DB
    // - Client gửi lên: selectedThIds + flags (idThMp,timeSec)
    // ================================
    public class RandomKetQuaRequest
    {
        public List<int> SelectedThIds { get; set; } = new();
        public List<FlagItem> Flags { get; set; } = new();
    }

    [HttpPost]
    public async Task<IActionResult> LuuKetQuaNgauNhien([FromBody] RandomKetQuaRequest request)
    {
        if (request == null) return BadRequest("Request null.");
        if (request.SelectedThIds == null || request.SelectedThIds.Count == 0)
            return BadRequest("Thiếu SelectedThIds.");

        // Load các tình huống theo ids
        var ths = await _context.TinhHuongMoPhongs
            .Where(t => request.SelectedThIds.Contains(t.IdThMp))
            .ToListAsync();

        var thMap = ths.ToDictionary(t => t.IdThMp, t => t);

        // Normalize flags: mỗi IdThMp chỉ lấy 1 cái (lấy cái đầu tiên theo time)
        var flags = NormalizeFlags(request.Flags ?? new List<FlagItem>());

        int tongDiem = 0;

        // Chỉ chấm trên danh sách 10 câu random
        foreach (var idTh in request.SelectedThIds.Distinct())
        {
            if (!thMap.TryGetValue(idTh, out var th)) continue;

            double startSec = FrameToSec(th.TgBatDau ?? 0);
            double endSec = FrameToSec(th.TgKetThuc ?? 0);

            // Nếu user không bấm thì 0 điểm
            var f = flags.FirstOrDefault(x => x.IdThMp == idTh);
            double timePress = f?.TimeSec ?? 0;

            tongDiem += TinhDiemTheoThoiDiem(timePress, startSec, endSec);
        }

        bool dat = tongDiem >= 35;

        return Ok(new
        {
            success = true,
            tongDiem,
            dat,
            isRandom = true
        });
    }
}
