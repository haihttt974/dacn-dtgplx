using dacn_dtgplx.ViewModels.Report;

namespace dacn_dtgplx.ViewModels.Reports
{
    public class TestsSectionVM
    {
        // ===== KPI tổng =====
        public int TongLuotThiLyThuyet { get; set; }          // count BaiLam
        public int TongLuotThiMoPhong { get; set; }           // count BaiLamMoPhong
        public int HocVienThiLyThuyet { get; set; }           // distinct BaiLam.UserId
        public int HocVienThiMoPhong { get; set; }            // distinct BaiLamMoPhong.UserId

        // ===== Charts theo tháng =====

        // 1) Lượt thi theo tháng
        public MultiSeriesChartVM AttemptsByMonth_Multi { get; set; } = new()
        {
            Title = "Lượt làm bài theo tháng (Lý thuyết vs Mô phỏng)"
        };

        // 2) Học viên tham gia theo tháng (distinct)
        public MultiSeriesChartVM UsersByMonth_Multi { get; set; } = new()
        {
            Title = "Số học viên làm bài theo tháng (Lý thuyết vs Mô phỏng)"
        };

        // ===== Pie breakdown =====

        // Lý thuyết theo Hạng (BoDeThiThu.IdHangNavigation.MaHang/TenDayDu)
        public List<PieSliceVM> TheoryByHangPie { get; set; } = new();

        // Mô phỏng theo Bộ đề mô phỏng (BoDeMoPhong.TenBoDe)
        public List<PieSliceVM> SimulationByBoDePie { get; set; } = new();

        // ===== Tables =====
        public List<TestMonthRowVM> TestsByMonthTable { get; set; } = new();
    }
}
