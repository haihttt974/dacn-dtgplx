using System;
using System.Threading.Tasks;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DtGplxContext _context;

        public ChatHub(DtGplxContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gửi tin nhắn trong 1 conversation.
        /// JS gọi: connection.invoke("SendMessage", conversationId, message)
        /// </summary>
        public async Task SendMessage(int conversationId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            // Lấy user hiện tại từ Claim (NameIdentifier = UserId)
            if (!int.TryParse(Context.UserIdentifier, out int senderId))
                return;

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationsId == conversationId &&
                    (c.UserId == senderId || c.UserId2 == senderId));

            if (conversation == null)
                return;

            var receiverId = conversation.UserId == senderId
                ? conversation.UserId2
                : conversation.UserId;

            var newMessage = new Message
            {
                ConversationsId = conversationId,
                UserId = senderId,
                MessageText = message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(newMessage);
            conversation.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var sentTime = newMessage.SentAt.ToLocalTime().ToString("HH:mm dd/MM");

            // Gửi cho cả người gửi và người nhận (2 phía đều realtime)
            await Clients.Users(senderId.ToString(), receiverId.ToString())
                .SendAsync("ReceiveMessage", conversationId, senderId, newMessage.MessageText, sentTime);

            // Cho tất cả client biết hội thoại này vừa có cập nhật
            await Clients.All.SendAsync("ConversationUpdated", conversationId);
        }
    }
}
