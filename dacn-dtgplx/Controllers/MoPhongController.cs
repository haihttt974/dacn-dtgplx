using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;

namespace dacn_dtgplx.Controllers
{
    public class MoPhongController : Controller
    {
        private readonly DtGplxContext _context;

        public MoPhongController(DtGplxContext context)
        {
            _context = context;
        }

        // ================================
        // ÔN TẬP TẤT CẢ TÌNH HUỐNG MÔ PHỎNG
        // ================================
        public async Task<IActionResult> Index()
        {
            /*
             * Load:
             *  - Chương mô phỏng
             *  - Tình huống theo từng chương
             *  - KHÔNG ràng buộc bộ đề
             *  - KHÔNG lưu kết quả
             */

            var chuongs = await _context.ChuongMoPhongs
                .Include(c => c.TinhHuongMoPhongs)
                .OrderBy(c => c.ThuTu)
                .Select(c => new ChuongMoPhongVm
                {
                    IdChuongMp = c.IdChuongMp,
                    TenChuong = c.TenChuong ?? "",

                    TinhHuongs = c.TinhHuongMoPhongs
                        .OrderBy(t => t.ThuTu)
                        .Select(th => new TinhHuongItem2
                        {
                            IdThMp = th.IdThMp,
                            TieuDe = th.TieuDe ?? $"Tình huống #{th.IdThMp}",
                            VideoUrl = NormalizeStaticPath(th.VideoUrl),
                            HintImageUrl = NormalizeStaticPath(th.UrlAnhMeo),

                            // ⚠️ DB lưu FRAME → convert sang GIÂY
                            ScoreStartSec = (th.TgBatDau ?? 0) / 60.0,
                            ScoreEndSec = (th.TgKetThuc ?? 0) / 60.0,
                            Kho = th.Kho ?? false
                        })
                        .ToList()
                })
                .ToListAsync();

            var vm = new OnTapMoPhongViewModel
            {
                Chuongs = chuongs
            };

            return View(vm);
        }

        // ================================
        // HELPERS
        // ================================
        private static string NormalizeStaticPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            if (path.StartsWith("wwwroot"))
                path = path.Substring("wwwroot".Length);

            if (!path.StartsWith("/"))
                path = "/" + path;

            return path.Replace("\\", "/");
        }
    }
}
