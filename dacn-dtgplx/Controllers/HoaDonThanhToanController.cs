using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class HoaDonThanhToanController : Controller
{
    private readonly DtGplxContext _context;

    public HoaDonThanhToanController(DtGplxContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var payments = await _context.HoaDonThanhToans
            .Include(h => h.IdDangKyNavigation)
                .ThenInclude(dk => dk.KhoaHoc)
            .Include(h => h.IdDangKyNavigation.HoSo)
            .Where(h => h.IdDangKyNavigation.HoSo.UserId == userId)
            .OrderByDescending(h => h.NgayThanhToan)
            .ToListAsync();

        return View(payments);
    }
}
