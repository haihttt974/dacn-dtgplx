using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace dacn_dtgplx.Controllers
{
    public class ConversationController : Controller
    {
        private readonly DtGplxContext _context;

        public ConversationController(DtGplxContext context)
        {
            _context = context;
        }

        // ================================
        // 1) Trang danh sách cuộc trò chuyện
        // ================================
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var conversations = _context.Conversations
                .Where(c => c.UserId == userId.Value || c.UserId2 == userId.Value)
                .Include(c => c.Messages)
                .Include(c => c.User)
                .Include(c => c.UserId2Navigation)
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return View(conversations);
        }

        // ================================
        // 2) Lấy số tin nhắn chưa đọc
        // ================================
        [HttpGet]
        public IActionResult CountUnread()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(0);

            int uid = userId.Value;

            var unread = _context.Messages
                .Where(m =>
                    m.UserId != uid &&              // tin do người khác gửi
                    !m.IsRead &&                   // chưa đọc
                    (
                        m.Conversations.UserId == uid ||
                        m.Conversations.UserId2 == uid
                    )
                )
                .Count();

            return Json(unread);
        }

        // ================================
        // 3) Màn hình xem chi tiết một cuộc trò chuyện
        // ================================
        public IActionResult Chat(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            int uid = userId.Value;

            var conversation = _context.Conversations
                .Include(c => c.User)
                .Include(c => c.UserId2Navigation)
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                .FirstOrDefault(c => c.ConversationsId == id);

            if (conversation == null)
                return NotFound();

            // Đánh dấu tin nhắn đã xem
            var unreadMessages = conversation.Messages
                .Where(m => m.UserId != uid && !m.IsRead)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }

                _context.SaveChanges();
            }

            return View(conversation);
        }

        // ================================
        // 4) API gửi tin nhắn
        // ================================
        [HttpPost]
        public IActionResult SendMessage(int conversationId, string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, error = "not_logged_in" });

            int uid = userId.Value;

            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, error = "empty_message" });

            var conversation = _context.Conversations
                .FirstOrDefault(c => c.ConversationsId == conversationId);

            if (conversation == null)
                return Json(new { success = false, error = "conversation_not_found" });

            var msg = new Message
            {
                ConversationsId = conversationId,
                UserId = uid,
                MessageText = message.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(msg);

            conversation.LastMessageAt = DateTime.Now;

            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
