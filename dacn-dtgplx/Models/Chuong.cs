using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class Chuong
{
    public int ChuongId { get; set; }

    public string? TenChuong { get; set; }

    public int? ThuTu { get; set; }

    public virtual ICollection<CauHoiLyThuyet> CauHoiLyThuyets { get; set; } = new List<CauHoiLyThuyet>();
}
