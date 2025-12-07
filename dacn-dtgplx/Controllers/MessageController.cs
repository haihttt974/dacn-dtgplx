using System;
using System.Linq;
using System.Threading.Tasks;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class MessageController : Controller
    {
        private readonly DtGplxContext _db;

        public MessageController(DtGplxContext db)
        {
            _db = db;
        }

        // ======================= VIEW A: DANH SÁCH HỘI THOẠI =======================
        public async Task<IActionResult> Conversations()
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out int userId))
                return RedirectToAction("Login", "Auth");

            var conversations = await _db.Conversations
                .Include(c => c.Messages)
                .Include(c => c.User)
                .Include(c => c.UserId2Navigation)
                .Where(c => c.UserId == userId || c.UserId2 == userId)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return View(conversations);
        }

        // ======================= VIEW B: CHAT 1-1 =======================
        // /Message/Chat?conversationId=5
        public async Task<IActionResult> Chat(int conversationId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out int userId))
                return RedirectToAction("Login", "Auth");

            var convo = await _db.Conversations
                .Include(c => c.User)
                .Include(c => c.UserId2Navigation)
                .FirstOrDefaultAsync(c => c.ConversationsId == conversationId);

            if (convo == null) return NotFound();

            var otherUser = convo.UserId == userId ? convo.UserId2Navigation : convo.User;

            ViewBag.ConversationId = convo.ConversationsId;
            ViewBag.OtherName = otherUser.TenDayDu ?? otherUser.Username;

            return View();
        }

        // ======================= API: Lấy tin nhắn 1 conversation ===================
        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out int userId))
                return Unauthorized();

            // Optional: kiểm tra user có quyền xem hội thoại này
            var exists = await _db.Conversations
                .AnyAsync(c => c.ConversationsId == conversationId &&
                               (c.UserId == userId || c.UserId2 == userId));
            if (!exists) return Unauthorized();

            var messages = await _db.Messages
                .Where(m => m.ConversationsId == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    senderUserId = m.UserId,
                    messageText = m.MessageText,
                    sentAt = m.SentAt
                })
                .ToListAsync();

            return Json(messages);
        }

        // ======================= API: Tổng số chưa đọc / hội thoại =================
        [HttpGet]
        public async Task<IActionResult> UnreadPerConversation()
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out int userId))
                return Unauthorized();

            var data = await _db.Messages
                .Where(m => m.UserId != userId && !m.IsRead)
                .GroupBy(m => m.ConversationsId)
                .Select(g => new
                {
                    conversationId = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Json(data);
        }

        // ======================= API: Đánh dấu đã đọc =============================
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out int userId))
                return Unauthorized();

            var unread = await _db.Messages
                .Where(m => m.ConversationsId == conversationId &&
                            m.UserId != userId &&
                            !m.IsRead)
                .ToListAsync();

            foreach (var m in unread)
                m.IsRead = true;

            await _db.SaveChangesAsync();

            return Json(new { success = true, count = unread.Count });
        }

        // ======================= API: Preview tin mới nhất ==========================
        [HttpGet]
        public async Task<IActionResult> GetConversationPreview(int conversationId)
        {
            var convo = await _db.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationsId == conversationId);

            if (convo == null) return NotFound();

            var lastMsg = convo.Messages
                .OrderByDescending(m => m.SentAt)
                .Select(m => new
                {
                    text = m.MessageText,
                    sentAt = m.SentAt.ToLocalTime().ToString("HH:mm dd/MM")
                })
                .FirstOrDefault();

            return Json(new
            {
                conversationId = convo.ConversationsId,
                lastMessage = lastMsg?.text ?? "",
                time = lastMsg?.sentAt ?? ""
            });
        }

        // ======================= API: XÓA HỘI THOẠI ================================
        [HttpPost]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out int userId))
                return Unauthorized();

            var convo = await _db.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationsId == id &&
                                          (c.UserId == userId || c.UserId2 == userId));

            if (convo == null)
                return Json(new { success = false });

            _db.Messages.RemoveRange(convo.Messages);
            _db.Conversations.Remove(convo);
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
