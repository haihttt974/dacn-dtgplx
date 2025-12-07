using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    public class HocController : Controller
    {
        private readonly DtGplxContext _context;

        public HocController(DtGplxContext context)
        {
            _context = context;
        }
        public IActionResult Index(bool open = false)
        {
            var listHang = _context.Hangs.ToList();
            var selectedHang = HttpContext.Session.GetString("Hang");
            ViewBag.ListHang = listHang;
            ViewBag.SelectedHang = selectedHang;
            if (open)
            {
                ViewBag.ShowPopup = true;
                return View();
            }
            ViewBag.ShowPopup = string.IsNullOrEmpty(selectedHang);

            return View();
        }

        [HttpPost]
        public IActionResult ChonHang(string maHang)
        {
            if (!string.IsNullOrEmpty(maHang))
                HttpContext.Session.SetString("Hang", maHang);

            return RedirectToAction("Index");
        }
    }
}
