using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class TinhHuongMoPhong
{
    public int IdThMp { get; set; }

    public int IdChuongMp { get; set; }

    public string? TieuDe { get; set; }

    public string? VideoUrl { get; set; }

    public int? ThuTu { get; set; }

    public bool? Kho { get; set; }

    public double? TgBatDau { get; set; }

    public double? TgKetThuc { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? UrlAnhMeo { get; set; }

    public virtual ICollection<ChiTietBoDeMoPhong> ChiTietBoDeMoPhongs { get; set; } = new List<ChiTietBoDeMoPhong>();

    public virtual ICollection<DiemTungTinhHuong> DiemTungTinhHuongs { get; set; } = new List<DiemTungTinhHuong>();

    public virtual ChuongMoPhong IdChuongMpNavigation { get; set; } = null!;
}
