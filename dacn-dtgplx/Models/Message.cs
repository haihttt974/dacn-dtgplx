using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ConversationsId { get; set; }

    public int UserId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual Conversation Conversations { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
