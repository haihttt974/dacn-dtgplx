namespace dacn_dtgplx.ViewModels
{
    public class FlashCardSignSummaryVM
    {
        public int IdBienBao { get; set; }
        public string TenBienBao { get; set; } = null!;
        public string? Ynghia { get; set; }
        public string? HinhAnh { get; set; }

        public int SoDanhGia { get; set; }
    }
}
