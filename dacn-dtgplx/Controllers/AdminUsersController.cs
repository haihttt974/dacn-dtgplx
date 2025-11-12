using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dacn_dtgplx.Models;

namespace dacn_dtgplx.Controllers
{
    [Route("admin/users")]
    public class AdminUsersController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminUsersController(DtGplxContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? roleId)
        {
            var users = _context.Users.Include(u => u.Role).AsQueryable();
            if (roleId.HasValue && roleId.Value > 0)
                users = users.Where(u => u.RoleId == roleId);

            ViewBag.Roles = await _context.Roles.ToListAsync();
            ViewBag.CurrentRoleId = roleId; // <-- truyền ra View

            return View(await users.ToListAsync());
        }
    }
}
