using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dacn_dtgplx.Models;

namespace dacn_dtgplx.Controllers
{
    public class AdminResultsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminResultsController(DtGplxContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            ViewBag.Hangs = await _context.Hangs.OrderBy(h => h.MaHang).ToListAsync();
            return View();
        }
        public async Task<IActionResult> LoadExamSetsByHang(int idHang)
        {
            var boDes = await _context.BoDeThiThus
                .Where(b => b.IdHang == idHang)
                .OrderBy(b => b.TenBoDe)
                .ToListAsync();

            return Json(boDes);
        }
        public async Task<IActionResult> Filter(int? idHang, int? idBoDe)
        {
            var query = _context.BaiLams
                .Include(b => b.IdBoDeNavigation)
                    .ThenInclude(b => b.IdHangNavigation)
                .Include(b => b.User)
                .AsQueryable();

            if (idHang.HasValue)
                query = query.Where(b => b.IdBoDeNavigation.IdHang == idHang.Value);

            if (idBoDe.HasValue)
                query = query.Where(b => b.IdBoDe == idBoDe.Value);

            var results = await query
                .OrderByDescending(b => b.BaiLamId)
                .ToListAsync();

            return PartialView("_ResultsTable", results);
        }

        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int id)
        {
            var baiLam = await _context.BaiLams
                .Include(b => b.IdBoDeNavigation)
                    .ThenInclude(b => b.IdHangNavigation)
                .Include(b => b.User)
                .Include(b => b.ChiTietBaiLams)
                    .ThenInclude(ct => ct.IdCauHoiNavigation)
                        .ThenInclude(c => c.DapAns)
                .FirstOrDefaultAsync(b => b.BaiLamId == id);

            if (baiLam == null)
            {
                return NotFound();
            }

            return View(baiLam);
        }
    }
}
