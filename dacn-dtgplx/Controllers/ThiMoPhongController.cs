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
        var dsBoDe = await _context.BoDeMoPhongs
            .Where(b => b.IsActive == true)
            .Select(b => new BoDeMoPhongViewModel
            {
                IdBoDe = b.IdBoDeMoPhong,
                TenBoDe = b.TenBoDe,
                SoTinhHuong = b.SoTinhHuong ?? 10
            })
            .ToListAsync();

        return View(dsBoDe);
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
}
