namespace dacn_dtgplx.ViewModels
{
    public class BoDeMoPhongViewModel
    {
        public int IdBoDe { get; set; }
        public string? TenBoDe { get; set; }
        public int SoTinhHuong { get; set; }
        public bool HasResult { get; set; }          // có bài làm hay chưa
        public int TongDiem { get; set; }            // tổng điểm / 50
        public bool KetQua { get; set; }             // đậu/rớt
        public int SoTinhHuongSai { get; set; }      // sai = bấm ngoài khoảng
        public int? IdBaiLamMoiNhat { get; set; }    // để sau gắn nút xem lịch sử/review
    }
}
