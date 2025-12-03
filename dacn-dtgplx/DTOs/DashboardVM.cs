using dacn_dtgplx.Models;

namespace BackendAPI.DTOs
{
    public class DashboardVM
    {
        // 4 box đầu
        public int TongNguoiDung { get; set; }
        public int TongHang { get; set; }
        public int TongBoDe { get; set; }
        public int TongBaiLam { get; set; }

        // Biểu đồ bài làm theo hạng
        public List<string> HangLabels { get; set; }
        public List<int> SoBaiLamTheoHang { get; set; }

        // Biểu đồ người dùng active/inactive
        public int UserActive { get; set; }
        public int UserInactive { get; set; }

        // 5 người mới nhất
        public List<User> RecentUsers { get; set; }
    }
}
