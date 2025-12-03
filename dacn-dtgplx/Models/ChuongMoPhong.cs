using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class ChuongMoPhong
{
    public int IdChuongMp { get; set; }

    public string? TenChuong { get; set; }

    public int? ThuTu { get; set; }

    public virtual ICollection<TinhHuongMoPhong> TinhHuongMoPhongs { get; set; } = new List<TinhHuongMoPhong>();
}
