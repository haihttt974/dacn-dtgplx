namespace dacn_dtgplx.ViewModels
{
    public class SentNotificationItem
    {
        public int ThongBaoId { get; set; }
        public string TieuDe { get; set; }
        public DateTime TaoLuc { get; set; }
        public string NoiDung { get; set; }
        public List<NguoiNhanItem> NguoiNhans { get; set; } = new();
    }
}
