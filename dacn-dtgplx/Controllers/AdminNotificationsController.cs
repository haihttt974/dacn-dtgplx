using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminNotificationsController : Controller
{
    private readonly DtGplxContext _context;

    public AdminNotificationsController(DtGplxContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        int currentUserId = int.Parse(User.FindFirst("UserId").Value);

        var model = new GuiThongBaoViewModel
        {
            CurrentUserId = currentUserId,

            Roles = await _context.Roles
                .Select(r => new RoleItem
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                }).ToListAsync(),

            Users = await _context.Users
                .Where(u => u.UserId != currentUserId)
                .Select(u => new UserItem
                {
                    UserId = u.UserId,
                    FullName = u.TenDayDu,
                    RoleId = u.RoleId,
                    Email = u.Email,
                    Phone = u.SoDienThoai
                }).ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GuiThongBao(GuiThongBaoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // Ép SelectedRoleIds và SelectedUserIds không null
        model.SelectedRoleIds ??= new List<int>();
        model.SelectedUserIds ??= new List<int>();

        // 1) Lưu thông báo
        var thongBao = new ThongBao
        {
            TieuDe = model.TieuDe,
            NoiDung = model.NoiDung,
            SendRole = model.SelectedRoleIds.Any()
                ? string.Join(",", model.SelectedRoleIds)
                : null,
            TaoLuc = DateTime.UtcNow
        };

        _context.ThongBaos.Add(thongBao);
        await _context.SaveChangesAsync();

        // 2) User theo role
        List<int> usersTheoRole = new();

        if (model.SelectedRoleIds.Any())
        {
            usersTheoRole = await _context.Users
                .Where(u =>
                    u.UserId != model.CurrentUserId &&     // loại bản thân
                    u.RoleId != null &&
                    model.SelectedRoleIds.Contains(u.RoleId.Value))
                .Select(u => u.UserId)
                .ToListAsync();
        }

        // 3) Tổng hợp user (unique)
        var allUserIds = usersTheoRole
            .Concat(model.SelectedUserIds)
            .Where(id => id != model.CurrentUserId) // loại bản thân
            .Distinct()
            .ToList();

        // 4) Lưu bảng CtThongBao
        foreach (var uid in allUserIds)
        {
            _context.CtThongBaos.Add(new CtThongBao
            {
                UserId = uid,
                ThongBaoId = thongBao.ThongBaoId,
                ThoiGianGui = DateTime.UtcNow,
                DaXem = false
            });
        }

        await _context.SaveChangesAsync();

        // 5) Tạo message thành công

        // Tổng user (không tính bản thân)
        int totalUsers = await _context.Users
            .CountAsync(u => u.UserId != model.CurrentUserId);

        string successMessage;

        bool coRole = model.SelectedRoleIds.Any();
        bool laTatCa = allUserIds.Count == totalUsers;

        if (coRole)
        {
            // Lấy tên role
            var roleNames = await _context.Roles
                .Where(r => model.SelectedRoleIds.Contains(r.RoleId))
                .Select(r => r.RoleName)
                .ToListAsync();

            successMessage =
                $"Đã gửi thông báo tới vai trò: {string.Join(", ", roleNames)} (tổng {allUserIds.Count} người).";
        }
        else if (laTatCa)
        {
            successMessage = "Đã gửi thông báo tới TẤT CẢ người dùng.";
        }
        else
        {
            successMessage = $"Đã gửi thông báo tới {allUserIds.Count} người dùng được chọn.";
        }

        TempData["Success"] = successMessage;

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> SentList()
    {
        var list = await _context.ThongBaos
            .OrderByDescending(t => t.TaoLuc)
            .Select(t => new SentNotificationItem
            {
                ThongBaoId = t.ThongBaoId,
                TieuDe = t.TieuDe,
                NoiDung = t.NoiDung,
                TaoLuc = t.TaoLuc,
                NguoiNhans = t.CtThongBaos
                    .Select(ct => new NguoiNhanItem
                    {
                        UserId = ct.UserId,
                        FullName = ct.User.TenDayDu,
                        DaXem = ct.DaXem,
                        ThoiGianGui = ct.ThoiGianGui
                    }).ToList()
            })
            .ToListAsync();

        return View(list);
    }

}
