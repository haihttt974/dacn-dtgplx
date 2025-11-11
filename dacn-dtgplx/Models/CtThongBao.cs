using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class CtThongBao
{
    public int UserId { get; set; }

    public int ThongBaoId { get; set; }

    public DateTime ThoiGianGui { get; set; }

    public bool DaXem { get; set; }

    public virtual ThongBao ThongBao { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
