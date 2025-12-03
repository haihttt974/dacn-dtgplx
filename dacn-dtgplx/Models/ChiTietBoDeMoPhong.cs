using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class ChiTietBoDeMoPhong
{
    public int IdBoDeMoPhong { get; set; }

    public int IdThMp { get; set; }

    public int? ThuTu { get; set; }

    public virtual BoDeMoPhong IdBoDeMoPhongNavigation { get; set; } = null!;

    public virtual TinhHuongMoPhong IdThMpNavigation { get; set; } = null!;
}
