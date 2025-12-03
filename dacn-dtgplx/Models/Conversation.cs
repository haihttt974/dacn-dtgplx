using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class Conversation
{
    public int ConversationsId { get; set; }

    public int UserId { get; set; }

    public int UserId2 { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;

    public virtual User UserId2Navigation { get; set; } = null!;
}
