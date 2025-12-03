using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class ChiTietBaiLam
{
    public int BaiLamId { get; set; }

    public int IdCauHoi { get; set; }

    public string? DapAnDaChon { get; set; }

    public bool? KetQuaCau { get; set; }

    public virtual BaiLam BaiLam { get; set; } = null!;

    public virtual CauHoiLyThuyet IdCauHoiNavigation { get; set; } = null!;
}
