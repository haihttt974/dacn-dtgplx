namespace dacn_dtgplx.ViewModels
{
    public class KetQuaThiViewModel
    {
        public int TongDiem { get; set; }
        public bool KetQua { get; set; }

        public List<ChiTietKetQuaItem> ChiTiet { get; set; } = new();
    }
}
