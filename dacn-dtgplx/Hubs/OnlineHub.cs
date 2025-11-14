using Microsoft.AspNetCore.SignalR;

namespace dacn_dtgplx.Hubs
{
    public class OnlineHub : Hub
    {
        private static int OnlineUsers = 0;
        private static HashSet<string> Usernames = new();

        public override Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext().Session.GetString("Username") ?? "Unknown";

            Usernames.Add(username);
            OnlineUsers++;

            Clients.All.SendAsync("UpdateOnlineUsers", OnlineUsers, Usernames.ToList());
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.GetHttpContext().Session.GetString("Username") ?? "Unknown";

            Usernames.Remove(username);
            OnlineUsers--;

            Clients.All.SendAsync("UpdateOnlineUsers", OnlineUsers, Usernames.ToList());
            return base.OnDisconnectedAsync(exception);
        }
    }

}
