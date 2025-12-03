using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class TtGiaoVien
{
    public int TtGiaoVienId { get; set; }

    public string? ChuyenMon { get; set; }

    public string? ChuyenDaoTao { get; set; }

    public DateOnly? NgayBatDauLam { get; set; }

    public string? LichDay { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
