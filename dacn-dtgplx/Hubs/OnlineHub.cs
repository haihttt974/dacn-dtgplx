using dacn_dtgplx.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Hubs
{
    public class OnlineHub : Hub
    {
        private readonly DtGplxContext _context;

        public OnlineHub(DtGplxContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            if (http == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            string? userIdStr = http.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) ||
                !int.TryParse(userIdStr, out int userId))
            {
                await base.OnConnectedAsync();
                return;
            }

            var existing = await _context.WebsocketConnections
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (existing == null)
            {
                _context.WebsocketConnections.Add(new WebsocketConnection
                {
                    UserId = userId,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IsOnline = true,
                    ClientInfo = Context.ConnectionId
                });
            }
            else
            {
                existing.IsOnline = true;
                existing.LastActivity = DateTime.UtcNow;
                existing.ClientInfo = Context.ConnectionId;
            }

            await _context.SaveChangesAsync();

            await SendOnlineCount();
            await Clients.All.SendAsync("UserStatusChanged", userId, true);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var http = Context.GetHttpContext();
            if (http != null)
            {
                string? userIdStr = http.Session.GetString("UserId");
                if (!string.IsNullOrWhiteSpace(userIdStr) &&
                    int.TryParse(userIdStr, out int userId))
                {
                    var existing = await _context.WebsocketConnections
                        .FirstOrDefaultAsync(x => x.UserId == userId);

                    if (existing != null)
                    {
                        existing.IsOnline = false;
                        existing.LastActivity = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        await Clients.All.SendAsync("UserStatusChanged", userId, false);
                    }
                }
            }

            await SendOnlineCount();
            await base.OnDisconnectedAsync(exception);
        }

        private async Task SendOnlineCount()
        {
            int count = await _context.WebsocketConnections
                .CountAsync(x => x.IsOnline);

            await Clients.All.SendAsync("ReceiveOnlineCount", count);
        }

        public async Task<bool> IsUserOnline(int userId)
        {
            var user = await _context.WebsocketConnections
                .FirstOrDefaultAsync(x => x.UserId == userId);

            return user != null && user.IsOnline;
        }
    }
}
