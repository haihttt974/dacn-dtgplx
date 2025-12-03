using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dacn_dtgplx.Models;

namespace dacn_dtgplx.Controllers
{
    public class AdminSimulationQuestionsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminSimulationQuestionsController(DtGplxContext context)
        {
            _context = context;
        }

        // ================ INDEX: DANH SÁCH TÌNH HUỐNG MÔ PHỎNG ================
        public async Task<IActionResult> Index(int? idChuongMp, int? isKho)
        {
            var query = _context.TinhHuongMoPhongs
                .Include(t => t.IdChuongMpNavigation)
                .AsQueryable();

            if (idChuongMp.HasValue)
            {
                query = query.Where(t => t.IdChuongMp == idChuongMp.Value);
            }

            if (isKho.HasValue)
            {
                if (isKho.Value == 1)
                    query = query.Where(t => t.Kho == true);
                else if (isKho.Value == 0)
                    query = query.Where(t => t.Kho == false || t.Kho == null);
            }

            var list = await query
                .OrderBy(t => t.ThuTu)
                .ThenBy(t => t.IdThMp)
                .ToListAsync();

            ViewBag.Chuongs = await _context.ChuongMoPhongs
                .OrderBy(c => c.ThuTu ?? c.IdChuongMp)
                .ToListAsync();

            ViewBag.SelectedChuong = idChuongMp;
            ViewBag.SelectedKho = isKho;

            return View(list);
        }
        // Các action Create/Edit/Details/Delete mình có thể thêm sau nếu bạn muốn
    }
}
