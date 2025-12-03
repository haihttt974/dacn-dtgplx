using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class BoDeThiThu
{
    public int IdBoDe { get; set; }

    public string? TenBoDe { get; set; }

    public int? ThoiGian { get; set; }

    public int? SoCauHoi { get; set; }

    public bool? HoatDong { get; set; }

    public DateTime? TaoLuc { get; set; }

    public int IdHang { get; set; }

    public virtual ICollection<BaiLam> BaiLams { get; set; } = new List<BaiLam>();

    public virtual ICollection<ChiTietBoDeTn> ChiTietBoDeTns { get; set; } = new List<ChiTietBoDeTn>();

    public virtual Hang IdHangNavigation { get; set; } = null!;
}
