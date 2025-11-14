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

        /// <summary>
        /// Client gọi 5s/lần để báo vẫn đang online
        /// </summary>
        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            // Lấy userId từ Session (nhớ set khi login)
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Chưa đăng nhập thì thôi, không làm gì
                return Ok(new { ok = false, reason = "guest" });
            }

            var now = DateTime.UtcNow;

            // Tìm connection của user này (1 user 1 record là đủ)
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

        [HttpGet("count")]
        public async Task<IActionResult> GetOnlineCount()
        {
            // Quy ước: online = IsOnline = 1 và hoạt động trong 1 phút gần nhất
            var threshold = DateTime.UtcNow.AddMinutes(-1);

            int count = await _context.WebsocketConnections
                .Where(x => x.IsOnline && x.LastActivity >= threshold)
                .CountAsync();

            return Json(new { count });
        }
    }
}
