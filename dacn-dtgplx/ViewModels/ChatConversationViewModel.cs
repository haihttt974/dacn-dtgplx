using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class ChatConversationViewModel
    {
        public int ConversationsId { get; set; }

        public int OtherUserId { get; set; }        // <-- thêm

        public string OtherName { get; set; } = "";

        public string OtherAvatar { get; set; } = "";

        public bool IsOnline { get; set; }

        public Message? LastMessage { get; set; }   // bạn đã dùng rồi
    }
}
