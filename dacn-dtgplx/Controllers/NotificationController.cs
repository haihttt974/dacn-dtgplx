using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class NotificationController : Controller
{
    private readonly DtGplxContext _context;

    public NotificationController(DtGplxContext context)
    {
        _context = context;
    }

    // ===============================
    // 📌 1) HIỆN DANH SÁCH THÔNG BÁO (TRANG RIÊNG)
    // ===============================

    public async Task<IActionResult> Index(int page = 1)
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToAction("Login", "Auth");

        int pageSize = 10;

        var query = _context.CtThongBaos
            .Where(x => x.UserId == uid)
            .OrderByDescending(x => x.ThoiGianGui);

        var totalCount = await query.CountAsync();
        var unreadCount = await query.CountAsync(x => !x.DaXem);

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationViewModel
            {
                ThongBaoId = x.ThongBaoId,
                TieuDe = x.ThongBao.TieuDe,
                NoiDung = x.ThongBao.NoiDung,
                ThoiGianGui = x.ThoiGianGui,
                DaXem = x.DaXem
            })
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.UnreadCount = unreadCount;
        ViewBag.FilterType = "all";

        return View(data);
    }

    // ========== CHỈ HIỂN THỊ CHƯA ĐỌC ========== //
    public async Task<IActionResult> Unread(int page = 1)
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToAction("Login", "Auth");

        int pageSize = 10;

        var query = _context.CtThongBaos
            .Where(x => x.UserId == uid && !x.DaXem)
            .OrderByDescending(x => x.ThoiGianGui);

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationViewModel
            {
                ThongBaoId = x.ThongBaoId,
                TieuDe = x.ThongBao.TieuDe,
                NoiDung = x.ThongBao.NoiDung,
                ThoiGianGui = x.ThoiGianGui,
                DaXem = x.DaXem
            })
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.FilterType = "unread";

        return View("Index", data);
    }

    // ===============================
    // 📌 2) API TRẢ VỀ LIST THÔNG BÁO (CHO DROPDOWN)
    // ===============================
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return Json(new { success = false });

        var data = await _context.CtThongBaos
            .Where(x => x.UserId == uid)
            .OrderByDescending(x => x.ThoiGianGui)
            .Select(x => new {
                x.DaXem,
                x.ThoiGianGui,
                x.ThongBao.TieuDe,
                x.ThongBao.NoiDung,
                x.ThongBao.ThongBaoId
            })
            .ToListAsync();

        return Json(new { success = true, data });
    }


    // ===============================
    // 📌 3) API ĐẾM SỐ THÔNG BÁO CHƯA ĐỌC
    // ===============================
    [HttpGet]
    public async Task<IActionResult> CountUnread()
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return Json(0);

        int count = await _context.CtThongBaos
            .CountAsync(x => x.UserId == uid && !x.DaXem);

        return Json(count);
    }


    // ========== ĐÁNH DẤU ĐÃ ĐỌC ========== //
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return Json(new { success = false });

        var item = await _context.CtThongBaos
            .FirstOrDefaultAsync(x => x.UserId == uid && x.ThongBaoId == id);

        if (item != null)
        {
            item.DaXem = true;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // ========== ĐÁNH DẤU TẤT CẢ ĐÃ ĐỌC ========== //
    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToAction("Login", "Auth");

        var list = await _context.CtThongBaos.Where(x => x.UserId == uid).ToListAsync();
        foreach (var item in list)
            item.DaXem = true;

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ========== XÓA THÔNG BÁO ========== //
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToAction("Login", "Auth");

        var item = await _context.CtThongBaos
            .FirstOrDefaultAsync(x => x.UserId == uid && x.ThongBaoId == id);

        if (item != null)
        {
            _context.CtThongBaos.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // ========== XÓA TẤT CẢ ========== //
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        int? uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToAction("Login", "Auth");

        var list = _context.CtThongBaos.Where(x => x.UserId == uid);
        _context.CtThongBaos.RemoveRange(list);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}