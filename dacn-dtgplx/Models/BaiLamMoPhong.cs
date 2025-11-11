using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class BaiLamMoPhong
{
    public int IdBaiLamTongDiem { get; set; }

    public int? TongDiem { get; set; }

    public bool? KetQua { get; set; }

    public int UserId { get; set; }

    public int IdBoDeMoPhong { get; set; }

    public virtual ICollection<DiemTungTinhHuong> DiemTungTinhHuongs { get; set; } = new List<DiemTungTinhHuong>();

    public virtual BoDeMoPhong IdBoDeMoPhongNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
