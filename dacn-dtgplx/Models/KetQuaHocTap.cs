using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class KetQuaHocTap
{
    public int KqHocTapId { get; set; }

    public string? NhanXet { get; set; }

    public int? SoBuoiDaHoc { get; set; }

    public int? SoBuoiToiThieu { get; set; }

    public int? KmHoanThanh { get; set; }

    public int? GioBanDem { get; set; }

    public bool? HtLyThuyet { get; set; }

    public bool? HtMoPhong { get; set; }

    public bool? HtSaHinh { get; set; }

    public bool? HtDuongTruong { get; set; }

    public bool? DuDkThiTn { get; set; }

    public bool? DauTn { get; set; }

    public bool? DuDkThiSh { get; set; }

    public DateTime ThoiGianCapNhat { get; set; }

    public int HoSoId { get; set; }

    public virtual HoSoThiSinh HoSo { get; set; } = null!;
}
