using System.Collections.Generic;
using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class FlashCardDetailVM
    {
        public int IdBienBao { get; set; }
        public string TenBienBao { get; set; } = null!;
        public string? Ynghia { get; set; }
        public string? HinhAnh { get; set; }

        public List<FlashCardItemVM> Items { get; set; } = new();
    }

    public class FlashCardItemVM
    {
        public int IdFlashcard { get; set; }
        public string? DanhGia { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
