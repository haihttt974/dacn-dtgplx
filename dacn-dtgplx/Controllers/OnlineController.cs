using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace dacn_dtgplx.Controllers
{
    [Route("online")]
    public class OnlineController : Controller
    {
        private readonly DtGplxContext _context;

        public OnlineController(DtGplxContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) PING – client gọi 5s/lần
        // ============================================================
        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Ok(new { ok = false, reason = "guest" });

            var now = DateTime.UtcNow;
            var conn = await _context.WebsocketConnections
                .FirstOrDefaultAsync(x => x.UserId == userId.Value);

            if (conn == null)
            {
                conn = new WebsocketConnection
                {
                    UserId = userId.Value,
                    ConnectedAt = now,
                    LastActivity = now,
                    IsOnline = true,
                    ClientInfo = HttpContext.Request.Headers["User-Agent"].ToString()
                };
                _context.WebsocketConnections.Add(conn);
            }
            else
            {
                conn.LastActivity = now;
                conn.IsOnline = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { ok = true });
        }

        // ============================================================
        // 2) ĐẾM SỐ ONLINE – Admin đang dùng
        // ============================================================
        [HttpGet("count")]
        public async Task<IActionResult> GetOnlineCount()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-1);

            int count = await _context.WebsocketConnections
                .Where(x => x.IsOnline && x.LastActivity >= threshold)
                .CountAsync();

            return Json(new { count });
        }

        // ============================================================
        // 3) LẤY DANH SÁCH USER ĐANG ONLINE – (Dùng cho Layout)
        // ============================================================
        [HttpGet("users")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-1);

            var data = await _context.WebsocketConnections
                .Where(x => x.IsOnline && x.LastActivity >= threshold)
                .Include(x => x.User)
                .OrderByDescending(x => x.LastActivity)
                .Select(x => new
                {
                    userId = x.UserId,
                    fullName = x.User.TenDayDu ?? x.User.Username,
                    email = x.User.Email,
                    avatar = string.IsNullOrWhiteSpace(x.User.Avatar)
                            ? "/images/avatar/default.png"
                            : "/" + x.User.Avatar.Replace("wwwroot/", "").TrimStart('/'),
                    lastActive = x.LastActivity.Value.ToLocalTime().ToString("HH:mm:ss dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(new { success = true, data });
        }

        // ============================================================
        // 4) TỰ ĐỘNG DỌN USER OFFLINE (nếu cần)
        // ============================================================
        [HttpPost("cleanup")]
        public async Task<IActionResult> Cleanup()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-1);

            var old = await _context.WebsocketConnections
                .Where(x => x.LastActivity < threshold)
                .ToListAsync();

            foreach (var conn in old)
                conn.IsOnline = false;

            await _context.SaveChangesAsync();
            return Ok(new { cleaned = old.Count });
        }
    }
}
