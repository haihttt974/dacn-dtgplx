using BackendAPI.DTOs;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace dacn_dtgplx.Controllers
{
    [Route("AdminDashboard")]
    public class AdminDashboardController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminDashboardController(DtGplxContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardVM();

            /* ==================== 4 BOX ĐẦU ==================== */
            vm.TongNguoiDung = await _context.Users.CountAsync();
            vm.TongHang = await _context.Hangs.CountAsync();
            vm.TongBoDe = await _context.BoDeThiThus.CountAsync();
            vm.TongBaiLam = await _context.BaiLams.CountAsync();

            /* ==================== BIỂU ĐỒ: Bài làm theo Hạng ==================== */
            vm.HangLabels = await _context.Hangs
                    .OrderBy(x => x.TenDayDu)
                    .Select(x => x.TenDayDu)
                    .ToListAsync();

            vm.SoBaiLamTheoHang = await _context.Hangs
                .OrderBy(x => x.TenDayDu)
                .Select(h => _context.BaiLams.Count(b => b.IdBoDeNavigation.IdHang == h.IdHang))
                .ToListAsync();

            /* ==================== BIỂU ĐỒ: User Active / Inactive ==================== */
            vm.UserActive = await _context.Users.CountAsync(x => x.TrangThai == true);
            vm.UserInactive = await _context.Users.CountAsync(x => x.TrangThai == false);

            /* ==================== 5 USER MỚI NHẤT ==================== */
            vm.RecentUsers = await _context.Users
                .OrderByDescending(u => u.TaoLuc)
                .Take(5)
                .ToListAsync();

            /* ==================== BIỂU ĐỒ Người dùng mới theo tháng ==================== */
            // Lấy 12 tháng gần nhất
            var now = DateTime.UtcNow;
            var start = now.AddMonths(-11);

            var query = await _context.Users
                .Where(u => u.TaoLuc >= start)
                .GroupBy(u => new { u.TaoLuc.Year, u.TaoLuc.Month })
                .Select(g => new
                {
                    Label = g.Key.Month + "/" + g.Key.Year,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.ThangNguoiDung = query.Select(x => x.Label).ToList();
            ViewBag.SoNguoiMoi = query.Select(x => x.Count).ToList();

            return View(vm);
        }
    }
}
