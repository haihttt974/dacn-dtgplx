using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class HoaDonThanhToan
{
    public int IdThanhToan { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public decimal? SoTien { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public bool? TrangThai { get; set; }

    public string? NoiDung { get; set; }

    public int? IdDangKy { get; set; }

    public int? PhieuTxId { get; set; }

    public virtual DangKyHoc? IdDangKyNavigation { get; set; }

    public virtual PhieuThueXe? PhieuTx { get; set; }
}
