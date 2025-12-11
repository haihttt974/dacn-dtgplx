namespace dacn_dtgplx.ViewModels
{
    public class LichDayItem
    {
        public int KhoaHocId { get; set; }

        public string TenKhoaHoc { get; set; }

        public DateTime NgayHoc { get; set; }   // convert từ DateOnly

        public string GioHoc { get; set; }      // "08:00 - 10:00" (TimeOnly)

        public string NoiDung { get; set; }

        public string DiaDiem { get; set; }
    }
}
