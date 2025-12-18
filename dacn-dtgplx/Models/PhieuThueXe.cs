using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dacn_dtgplx.Models;
[Table("PhieuThueXe")]
public partial class PhieuThueXe
{
    [Key]
    public int PhieuTxId { get; set; }

    public int UserId { get; set; }

    [Column("xeTapLaiId")]
    public int XeId { get; set; }

    [Column("tg_BatDau")]
    [Display(Name = "Thời gian bắt đầu")]
    public DateTime? TgBatDau { get; set; }

    [Column("tg_Thue")]
    [Display(Name = "Số giờ thuê")]
    public int? TgThue { get; set; }

    [Column("daLayXe")]
    [Display(Name = "Đã lấy xe")]
    public bool DaLayXe { get; set; } = false;
    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual XeTapLai Xe { get; set; } = null!;

    public virtual ICollection<HoaDonThanhToan> HoaDonThanhToans { get; set; } = new List<HoaDonThanhToan>();
}
