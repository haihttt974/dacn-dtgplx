using dacn_dtgplx.ViewModels.Report;

namespace dacn_dtgplx.ViewModels.Reports
{
    public class OverviewSectionVM
    {
        // ===== KPI doanh thu =====
        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuKhoaHoc { get; set; }
        public decimal DoanhThuThueXe { get; set; }

        // Số hóa đơn / giao dịch
        public int SoGiaoDich { get; set; }
        public int SoGiaoDichKhoaHoc { get; set; }
        public int SoGiaoDichThueXe { get; set; }

        // Doanh thu trung bình
        public decimal DoanhThuTrungBinhNgay { get; set; }
        public decimal DoanhThuTrungBinhThang { get; set; }

        // ===== Charts =====

        /// <summary>
        /// Biểu đồ multi-series theo tháng:
        /// - Series1: Doanh thu khóa học
        /// - Series2: Doanh thu thuê xe
        /// </summary>
        public MultiSeriesChartVM RevenueByMonth_Multi { get; set; } = new()
        {
            Title = "Doanh thu theo tháng (Khóa học vs Thuê xe)"
        };

        /// <summary>
        /// Biểu đồ pie cơ cấu doanh thu
        /// </summary>
        public List<PieSliceVM> RevenueSharePie { get; set; } = new();

        // ===== Tables =====
        public List<RevenueMonthRowVM> RevenueByMonthTable { get; set; } = new();

        // Top breakdown
        public List<PieSliceVM> TopCoursesByRevenue { get; set; } = new();    // Top khóa học theo doanh thu
        public List<PieSliceVM> TopVehiclesByRevenue { get; set; } = new();   // Top xe theo doanh thu (Biển số/Loại xe)
    }
}
