using dacn_dtgplx.ViewModels.Report;

namespace dacn_dtgplx.ViewModels.Reports
{
    public class UsersSectionVM
    {
        // KPI
        public int TongNguoiDung { get; set; }                 // tổng users (tuỳ bạn có muốn)
        public int SoNguoiMoi { get; set; }                    // theo filter
        public int SoHoSoMoi { get; set; }                     // số hồ sơ tạo mới (khác với người mới)

        // Chart: người mới theo tháng (distinct user)
        public List<ChartPointVM> NewUsersByMonth { get; set; } = new();

        // Chart: hồ sơ mới theo tháng
        public List<ChartPointVM> NewProfilesByMonth { get; set; } = new();

        // Table chi tiết theo tháng
        public List<UserMonthRowVM> UsersByMonthTable { get; set; } = new();

        // (Optional) breakdown theo giới tính/role nếu sau này bạn muốn
        public List<PieSliceVM> UsersByRolePie { get; set; } = new();
    }
}
