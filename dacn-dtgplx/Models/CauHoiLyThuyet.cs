using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class CauHoiLyThuyet
{
    public int IdCauHoi { get; set; }

    public int ChuongId { get; set; }

    public string? NoiDung { get; set; }

    public string? HinhAnh { get; set; }

    public bool? CauLiet { get; set; }

    public bool? ChuY { get; set; }

    public bool? XeMay { get; set; }

    public string? UrlAnhMeo { get; set; }

    public virtual ICollection<ChiTietBaiLam> ChiTietBaiLams { get; set; } = new List<ChiTietBaiLam>();

    public virtual ICollection<ChiTietBoDeTn> ChiTietBoDeTns { get; set; } = new List<ChiTietBoDeTn>();

    public virtual Chuong Chuong { get; set; } = null!;

    public virtual ICollection<DapAn> DapAns { get; set; } = new List<DapAn>();
}
