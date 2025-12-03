using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class BaiLam
{
    public int BaiLamId { get; set; }

    public int? ThoiGianLamBai { get; set; }

    public int? SoCauSai { get; set; }

    public bool? KetQua { get; set; }

    public int UserId { get; set; }

    public int IdBoDe { get; set; }

    public virtual ICollection<ChiTietBaiLam> ChiTietBaiLams { get; set; } = new List<ChiTietBaiLam>();

    public virtual BoDeThiThu IdBoDeNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
