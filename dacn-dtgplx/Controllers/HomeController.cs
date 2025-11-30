using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace dacn_dtgplx.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DtGplxContext _context;

        public HomeController(ILogger<HomeController> logger, DtGplxContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                Courses = await _context.KhoaHocs
                    .OrderBy(k => k.NgayBatDau)
                    .ToListAsync(),

                Instructors = await _context.TtGiaoViens
                    .Include(g => g.User) // nếu cần
                    .ToListAsync(),

                Testimonials = await _context.PhanHois
                    .Include(p => p.User)
                    .OrderByDescending(p => p.ThoiGianPh)
                    .Take(6)
                    .ToListAsync(),
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
