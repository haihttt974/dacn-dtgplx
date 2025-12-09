using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
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

        // --------------------------
        // 1. Hóa đơn khóa học
        // --------------------------
        var paymentsKhoaHoc = await _context.HoaDonThanhToans
            .Include(h => h.IdDangKyNavigation)
                .ThenInclude(dk => dk.KhoaHoc)
            .Include(h => h.IdDangKyNavigation.HoSo)
            .Where(h => h.IdDangKyNavigation.HoSo.UserId == userId)
            .OrderByDescending(h => h.NgayThanhToan)
            .ToListAsync();

        // --------------------------
        // 2. Hóa đơn thuê xe
        // --------------------------
        var paymentsThueXe = await _context.HoaDonThanhToans
            .Include(h => h.PhieuTx)
                .ThenInclude(p => p.Xe)
            .Where(h => h.PhieuTx != null && h.PhieuTx.UserId == userId)
            .OrderByDescending(h => h.NgayThanhToan)
            .ToListAsync();

        var vm = new PaymentHistoryVM
        {
            PaymentsKhoaHoc = paymentsKhoaHoc,
            PaymentsThueXe = paymentsThueXe
        };

        return View(vm);
    }
}
