using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class KhoaHoc
{
    public int KhoaHocId { get; set; }

    public string? TenKhoaHoc { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public int? SlToiDa { get; set; }

    public string? MoTa { get; set; }

    public bool? IsActive { get; set; }

    public int IdHang { get; set; }

    public virtual ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();

    public virtual Hang IdHangNavigation { get; set; } = null!;

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
