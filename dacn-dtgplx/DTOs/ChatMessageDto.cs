namespace dacn_dtgplx.DTOs
{
    public class ChatMessageDto
    {
        public bool IsMine { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
    }
}
