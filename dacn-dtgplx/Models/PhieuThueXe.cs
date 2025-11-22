using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.Models;

public partial class PhieuThueXe
{
    [Key]
    public int PhieuTxId { get; set; }

    public int UserId { get; set; }

    public int XeId { get; set; }

    public DateTime? TgBatDau { get; set; }

    public int? TgThue { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual XeTapLai Xe { get; set; } = null!;

    public virtual ICollection<HoaDonThanhToan> HoaDonThanhToans { get; set; } = new List<HoaDonThanhToan>();
}
