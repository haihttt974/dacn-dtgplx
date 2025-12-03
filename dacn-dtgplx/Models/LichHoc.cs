using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class LichHoc
{
    public int LichHocId { get; set; }

    public int? XeTapLaiId { get; set; }

    public int? LopHocId { get; set; }

    public int KhoaHocId { get; set; }

    public DateOnly NgayHoc { get; set; }

    public TimeOnly TgBatDau { get; set; }

    public TimeOnly TgKetThuc { get; set; }

    public string? NoiDung { get; set; }

    public string? DiaDiem { get; set; }

    public string? GhiChu { get; set; }

    public virtual KhoaHoc KhoaHoc { get; set; } = null!;

    public virtual LopHoc? LopHoc { get; set; }

    public virtual XeTapLai? XeTapLai { get; set; }
}
