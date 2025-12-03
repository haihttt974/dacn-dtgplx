using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class FlashCard
{
    public int IdFlashcard { get; set; }

    public string? DanhGia { get; set; }

    public int UserId { get; set; }

    public int IdBienBao { get; set; }

    public virtual BienBao IdBienBaoNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
