using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class PhanHoi
{
    public int PhanHoiId { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime ThoiGianPh { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
