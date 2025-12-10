using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class HoaDonThanhToanController : Controller
{
    private readonly DtGplxContext _context;
    private readonly IInvoiceService _invoiceService;
    private readonly IQrService _qrService;

    public HoaDonThanhToanController(DtGplxContext context, IInvoiceService invoiceService, IQrService qrService)
    {
        _context = context;
        _invoiceService = invoiceService;
        _qrService = qrService;
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

    public async Task<IActionResult> Detail(int id)
    {
        var bill = await _context.HoaDonThanhToans
            .Include(h => h.IdDangKyNavigation)
                .ThenInclude(d => d.KhoaHoc)
            .Include(h => h.IdDangKyNavigation)
                .ThenInclude(d => d.HoSo)
            .Include(h => h.PhieuTx)
                .ThenInclude(p => p.Xe)
            .Include(h => h.PhieuTx)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(h => h.IdThanhToan == id);

        if (bill == null) return NotFound();

        return PartialView("_HoaDonDetail", bill);
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
        var bill = await _context.HoaDonThanhToans
            .Include(h => h.IdDangKyNavigation)
                .ThenInclude(d => d.KhoaHoc)        // <<< THIẾU DÒNG NÀY
            .Include(h => h.IdDangKyNavigation)
                .ThenInclude(d => d.HoSo)
                    .ThenInclude(hs => hs.User)
            .Include(h => h.PhieuTx)
                .ThenInclude(p => p.User)
            .Include(h => h.PhieuTx)
                .ThenInclude(p => p.Xe)
            .FirstAsync(h => h.IdThanhToan == id);

        if (bill == null) return NotFound();

        byte[] pdf = _invoiceService.GenerateInvoicePdf(bill);
        return File(pdf, "application/pdf", $"HoaDon_{bill.IdThanhToan}.pdf");
    }
    public IActionResult ViewQr(int id)
    {
        string content = $"RENT-{id}";
        byte[] qr = _qrService.GenerateQrCode(content);

        string base64 = Convert.ToBase64String(qr);
        return Json(new { img = "data:image/png;base64," + base64 });
    }

    public IActionResult DownloadQr(int id)
    {
        string content = $"RENT-{id}";
        byte[] qr = _qrService.GenerateQrCode(content);

        return File(qr, "image/png", $"QR_Rent_{id}.png");
    }
}
