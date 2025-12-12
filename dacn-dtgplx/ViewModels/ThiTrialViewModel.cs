namespace dacn_dtgplx.ViewModels
{
    public class ThiTrialViewModel
    {
        public int IdBoDe { get; set; }

        // danh sách 10 tình huống của bộ đề
        public List<TinhHuongItem2> TinhHuongs { get; set; } = new();

        // tổng thời lượng video ghép (nếu bạn ghép sẵn thành file MP4)
        public double TongThoiLuong { get; set; }

        // file video ghép 10 tình huống
        public string VideoTongHopUrl { get; set; } = string.Empty;
    }
}
