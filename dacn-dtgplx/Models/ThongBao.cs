using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class ThongBao
{
    public int ThongBaoId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public DateTime TaoLuc { get; set; }

    public string? SendRole { get; set; }

    public virtual ICollection<CtThongBao> CtThongBaos { get; set; } = new List<CtThongBao>();
}
