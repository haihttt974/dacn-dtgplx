using dacn_dtgplx.DTOs;

namespace dacn_dtgplx.ViewModels
{
    public class ChatDetailViewModel
    {
        public int ConversationsId { get; set; }
        public int CurrentUserId { get; set; }
        public string OtherName { get; set; }
        public string OtherAvatar { get; set; }
        public List<ChatMessageDto> Messages { get; set; }
    }
}
