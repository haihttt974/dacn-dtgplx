using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminFeedbacksController : Controller
{
    private readonly DtGplxContext _context;

    public AdminFeedbacksController(DtGplxContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search = "")
    {
        var query = _context.PhanHois
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.NoiDung.Contains(search) ||
                p.User.TenDayDu.Contains(search) ||
                p.User.Email.Contains(search));
        }

        var items = await query
            .OrderByDescending(p => p.ThoiGianPh)
            .Select(p => new AdminFeedbackItem
            {
                PhanHoiId = p.PhanHoiId,
                NoiDung = p.NoiDung,
                ThoiGianPh = p.ThoiGianPh,
                SoSao = p.SoSao,       // KHÔNG cần check NULL nữa

                UserId = p.UserId,
                FullName = p.User.TenDayDu,
                Email = p.User.Email,
                Phone = p.User.SoDienThoai
            })
            .ToListAsync();

        return View(items);
    }
}
