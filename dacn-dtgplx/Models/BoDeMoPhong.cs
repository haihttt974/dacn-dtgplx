using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class BoDeMoPhong
{
    public int IdBoDeMoPhong { get; set; }

    public string? TenBoDe { get; set; }

    public int? SoTinhHuong { get; set; }

    public DateTime? TaoLuc { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<BaiLamMoPhong> BaiLamMoPhongs { get; set; } = new List<BaiLamMoPhong>();

    public virtual ICollection<ChiTietBoDeMoPhong> ChiTietBoDeMoPhongs { get; set; } = new List<ChiTietBoDeMoPhong>();
}
