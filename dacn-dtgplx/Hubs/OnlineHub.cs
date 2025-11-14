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
            var username = http.Session.GetString("Username");
            var userIdStr = http.Session.GetString("UserId");

            if (username == null || userIdStr == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            if (!int.TryParse(userIdStr, out int userId))
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
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var http = Context.GetHttpContext();
            var userIdStr = http.Session.GetString("UserId");

            if (userIdStr != null)
            {
                int userId = int.Parse(userIdStr);

                var existing = await _context.WebsocketConnections
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (existing != null)
                {
                    existing.IsOnline = false;
                    existing.LastActivity = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await SendOnlineCount();
            await base.OnDisconnectedAsync(exception);
        }

        private async Task SendOnlineCount()
        {
            int count = await _context.WebsocketConnections
                .Where(x => x.IsOnline)
                .CountAsync();

            await Clients.All.SendAsync("ReceiveOnlineCount", count);
        }
    }
}
