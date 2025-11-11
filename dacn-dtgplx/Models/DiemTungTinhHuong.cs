using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class DiemTungTinhHuong
{
    public int IdBaiLamTongDiem { get; set; }

    public int IdThMp { get; set; }

    public double ThoiDiemNguoiDungNhan { get; set; }

    public virtual BaiLamMoPhong IdBaiLamTongDiemNavigation { get; set; } = null!;

    public virtual TinhHuongMoPhong IdThMpNavigation { get; set; } = null!;
}
