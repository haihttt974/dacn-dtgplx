using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class QuyDinhHang
{
    public int QuyDinhHangId { get; set; }

    public int? KmToiThieu { get; set; }

    public int? SoGioBanDem { get; set; }

    public bool? LyThuyet { get; set; }

    public bool? SaHinh { get; set; }

    public bool? MoPhong { get; set; }

    public bool? DuongTruong { get; set; }

    public string? GhiChu { get; set; }

    public int IdHang { get; set; }

    public virtual Hang IdHangNavigation { get; set; } = null!;
}
