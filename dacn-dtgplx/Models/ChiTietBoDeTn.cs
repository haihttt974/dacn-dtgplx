using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class ChiTietBoDeTn
{
    public int IdBoDe { get; set; }

    public int IdCauHoi { get; set; }

    public int? ThuTu { get; set; }

    public virtual BoDeThiThu IdBoDeNavigation { get; set; } = null!;

    public virtual CauHoiLyThuyet IdCauHoiNavigation { get; set; } = null!;
}
