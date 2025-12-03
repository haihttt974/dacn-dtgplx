using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class HoSoThiSinh
{
    public int HoSoId { get; set; }

    public string? LoaiHoSo { get; set; }

    public DateOnly? NgayDk { get; set; }

    public string? KhamSucKhoe { get; set; }

    public string? GhiChu { get; set; }

    public int UserId { get; set; }

    public bool? DaDuyet { get; set; }

    public virtual ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();

    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();

    public virtual User User { get; set; } = null!;
}
