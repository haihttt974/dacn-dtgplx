using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class WebsocketConnection
{
    public long ConnectionId { get; set; }

    public DateTime ConnectedAt { get; set; }

    public DateTime? LastActivity { get; set; }

    public string? ClientInfo { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
