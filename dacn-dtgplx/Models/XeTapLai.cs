using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class XeTapLai
{
    public int XeTapLaiId { get; set; }

    public string LoaiXe { get; set; } = null!;

    public bool TrangThaiXe { get; set; }

    public string? BienSo { get; set; }   // thêm

    public decimal? GiaThueTheoGio { get; set; }   // thêm

    public string? AnhXe { get; set; }

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
