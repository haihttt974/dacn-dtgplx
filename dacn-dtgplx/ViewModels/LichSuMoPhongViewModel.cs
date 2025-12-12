namespace dacn_dtgplx.ViewModels
{
    public class LichSuMoPhongViewModel
    {
        public int IdBoDe { get; set; }
        public int IdBaiLam { get; set; }
        public int TongDiem { get; set; }
        public bool KetQua { get; set; }

        // giống BaiLam
        public List<TinhHuongItem2> TinhHuongs { get; set; } = new();

        // flags từ DB
        public List<ReviewFlagItem> Flags { get; set; } = new();
    }
}
