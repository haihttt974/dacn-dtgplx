namespace dacn_dtgplx.ViewModels
{
    public class NotificationViewModel
    {
        public int ThongBaoId { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianGui { get; set; }
        public bool DaXem { get; set; }

        // Các thuộc tính tiện ích cho View
        public string TimeAgo =>
            (DateTime.Now - ThoiGianGui).TotalMinutes < 1 ? "Vừa xong" :
            (DateTime.Now - ThoiGianGui).TotalHours < 1 ? $"{(int)(DateTime.Now - ThoiGianGui).TotalMinutes} phút trước" :
            (DateTime.Now - ThoiGianGui).TotalHours < 24 ? $"{(int)(DateTime.Now - ThoiGianGui).TotalHours} giờ trước" :
            ThoiGianGui.ToString("HH:mm dd/MM/yyyy");

        public bool IsRead => DaXem;

        // Optional (cho giao diện đẹp)
        public string IconClass => IsRead ? "fa-regular fa-bell" : "fa-solid fa-bell";
        public string ColorClass => IsRead ? "bg-secondary text-white" : "bg-primary text-white";

        public string Message => NoiDung;
        public string Title => TieuDe;
    }
}
