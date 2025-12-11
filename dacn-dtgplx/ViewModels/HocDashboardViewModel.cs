using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class HocDashboardViewModel
    {
        public string? SelectedHang { get; set; }
        public List<Hang> ListHang { get; set; } = new();

        public bool ShowPopup { get; set; }

        public int DoneBoDe { get; set; }
        public int TotalBoDe { get; set; }
        public int TotalCauHoi { get; set; }
        public int TotalCauLiet { get; set; }
        public int TotalCauChuY { get; set; }
        public int TotalBienBao { get; set; }

        // Mô phỏng
        public bool HasMoPhong { get; set; }
        public int MpBoDe { get; set; }
        public int MpBoDeDone { get; set; }
        public int MpTinhHuong { get; set; }
        public int ThoiGianThi { get; set; }
        public int SoCauThiNgauNhien { get; set; }
    }
}
