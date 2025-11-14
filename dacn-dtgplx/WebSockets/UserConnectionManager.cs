using System.Collections.Concurrent;

namespace dacn_dtgplx.WebSockets
{
    public static class UserConnectionManager
    {
        // userId -> connection count
        private static readonly ConcurrentDictionary<int, int> UserConnections = new();

        public static void AddConnection(int userId)
        {
            UserConnections.AddOrUpdate(userId, 1, (key, oldValue) => oldValue + 1);
        }

        public static void RemoveConnection(int userId)
        {
            UserConnections.AddOrUpdate(userId, 0, (key, oldValue) =>
            {
                var newValue = oldValue - 1;
                return newValue < 0 ? 0 : newValue;
            });
        }

        public static int GetOnlineUsers()
        {
            return UserConnections.Count(u => u.Value > 0);
        }
    }
}
