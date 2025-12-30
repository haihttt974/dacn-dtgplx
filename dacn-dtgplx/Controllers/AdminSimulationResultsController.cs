using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers.Admin
{
    public class AdminSimulationResultsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminSimulationResultsController(DtGplxContext context)
        {
            _context = context;
        }

        // ===================== DANH SÁCH BÀI LÀM =====================
        public async Task<IActionResult> Index()
        {
            var results = await _context.BaiLamMoPhongs
                .Include(x => x.User)
                .Include(x => x.IdBoDeMoPhongNavigation)
                .OrderByDescending(x => x.IdBaiLamTongDiem)
                .ToListAsync();

            // =================== TẠO DỮ LIỆU BIỂU ĐỒ ===================
            var stats = _context.BaiLamMoPhongs
                .AsEnumerable()
                .GroupBy(x => x.IdBaiLamTongDiem) // nếu bạn có thuộc tính thời gian => dùng CreatedAt.Date
                .Select(g => new
                {
                    Date = g.First().IdBaiLamTongDiem.ToString(), // tạm thời dùng ID do thiếu CreatedAt
                    Count = g.Count()
                })
                .ToList();

            SimulationResultStatsVM chart = new SimulationResultStatsVM();

            foreach (var s in stats)
            {
                chart.Labels.Add(s.Date);
                chart.Counts.Add(s.Count);
            }

            ViewBag.ChartData = chart;

            return View(results);
        }

        // ===================== CHI TIẾT BÀI LÀM =====================
        public async Task<IActionResult> Details(int id)
        {
            var baiLam = await _context.BaiLamMoPhongs
                .Include(bl => bl.User)

                // Bộ đề mô phỏng + thứ tự tình huống admin chọn
                .Include(bl => bl.IdBoDeMoPhongNavigation)
                    .ThenInclude(bd => bd.ChiTietBoDeMoPhongs)
                        .ThenInclude(ct => ct.IdThMpNavigation)

                // Điểm từng tình huống người dùng bấm
                .Include(bl => bl.DiemTungTinhHuongs)
                    .ThenInclude(d => d.IdThMpNavigation)

                .FirstOrDefaultAsync(bl => bl.IdBaiLamTongDiem == id);

            if (baiLam == null)
            {
                return NotFound();
            }

            return View(baiLam);
        }
    }
}
