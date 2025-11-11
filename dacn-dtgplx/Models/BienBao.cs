using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class BienBao
{
    public int IdBienBao { get; set; }

    public string TenBienBao { get; set; } = null!;

    public string? Ynghia { get; set; }

    public string? HinhAnh { get; set; }

    public virtual ICollection<FlashCard> FlashCards { get; set; } = new List<FlashCard>();
}
