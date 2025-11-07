using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}
