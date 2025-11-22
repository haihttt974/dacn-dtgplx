namespace dacn_dtgplx.ViewModels
{
    public class TinhHuongItem
    {
        public int IdThMp { get; set; }
        public string TieuDe { get; set; }
        public int ThuTuTrongChuong { get; set; }

        public string? AnhMeo { get; set; }
        public bool Kho { get; set; }

        public bool IsSelected { get; set; }   // đã nằm trong bộ đề
        public int? ThuTuTrongBoDe { get; set; } // thứ tự trong bộ đề
    }
}
