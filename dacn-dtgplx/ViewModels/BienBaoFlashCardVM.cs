namespace dacn_dtgplx.ViewModels
{
    public class BienBaoFlashCardVM
    {
        public int IdBienBao { get; set; }
        public string TenBienBao { get; set; } = "";
        public string? YNghia { get; set; }
        public string? HinhAnh { get; set; }

        // Flashcard của user (nếu có)
        public int? IdFlashcard { get; set; }
        public string? DanhGia { get; set; } // "Nho" | "ChuaNho"
    }
}
