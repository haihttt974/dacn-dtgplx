using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class LopHoc
{
    public int LopHocId { get; set; }

    public string TenLop { get; set; } = null!;

    public bool TrangThaiLop { get; set; }

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
