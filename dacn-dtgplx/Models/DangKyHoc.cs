using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class DangKyHoc
{
    public int IdDangKy { get; set; }

    public DateOnly? NgayDangKy { get; set; }

    public bool? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public int HoSoId { get; set; }

    public int KhoaHocId { get; set; }

    public virtual HoSoThiSinh HoSo { get; set; } = null!;

    public virtual ICollection<HoaDonThanhToan> HoaDonThanhToans { get; set; } = new List<HoaDonThanhToan>();

    public virtual KhoaHoc KhoaHoc { get; set; } = null!;
}
