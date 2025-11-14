using System.Net.WebSockets;
using System.Text;

namespace dacn_dtgplx.WebSockets
{
    public class UserWebSocketHandler
    {
        public static async Task Handle(HttpContext context, WebSocket webSocket)
        {
            if (!context.User.Identity.IsAuthenticated)
                return;

            int userId = int.Parse(context.User.FindFirst("UserId").Value);

            UserConnectionManager.AddConnection(userId);

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                }
            }
            finally
            {
                UserConnectionManager.RemoveConnection(userId);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
        }
    }
}
