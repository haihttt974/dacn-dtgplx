using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class Hang
{
    public int IdHang { get; set; }

    public string MaHang { get; set; } = null!;

    public string? TenDayDu { get; set; }

    public string? MoTa { get; set; }

    public int DiemDat { get; set; }

    public int ThoiGianTn { get; set; }

    public int SoCauHoi { get; set; }

    public DateTime TaoLuc { get; set; }

    public int? TuoiToiThieu { get; set; }

    public int? TuoiToiDa { get; set; }

    public string? SucKhoe { get; set; }

    public decimal? ChiPhi { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<BoDeThiThu> BoDeThiThus { get; set; } = new List<BoDeThiThu>();

    public virtual ICollection<KhoaHoc> KhoaHocs { get; set; } = new List<KhoaHoc>();

    public virtual ICollection<QuyDinhHang> QuyDinhHangs { get; set; } = new List<QuyDinhHang>();
}
