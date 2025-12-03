using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class DapAn
{
    public int IdDapAn { get; set; }

    public int IdCauHoi { get; set; }

    public string? NoiDung { get; set; }

    public bool DapAnDung { get; set; }

    public int ThuTu { get; set; }

    public virtual CauHoiLyThuyet IdCauHoiNavigation { get; set; } = null!;
}
