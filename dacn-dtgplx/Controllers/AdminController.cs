using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Bảng điều khiển - Quản trị hệ thống";

            // Dùng layout riêng cho admin
            ViewBag.Layout = "_LayoutAdmin";
            return View();
        }
    }
}
