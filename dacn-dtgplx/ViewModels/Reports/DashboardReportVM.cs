using dacn_dtgplx.ViewModels.Report;

namespace dacn_dtgplx.ViewModels.Reports
{
    public class DashboardReportVM
    {
        // ===== Filter =====
        public ReportFilterVM Filter { get; set; } = new();

        // Tab hiện tại (hữu ích khi export PDF theo tab)
        public ReportTab ActiveTab { get; set; } = ReportTab.Overview;

        // Text hiển thị “Thời gian: …”
        public string RangeText { get; set; } = "Tất cả";

        // ===== TOP KPI (hiển thị chung ở Overview, có thể tái dùng cho PDF) =====
        public List<KpiCardVM> TopKpis { get; set; } = new();

        // ===== OVERVIEW =====
        public OverviewSectionVM Overview { get; set; } = new();

        // ===== USERS =====
        public UsersSectionVM Users { get; set; } = new();

        // ===== COURSES =====
        public CoursesSectionVM Courses { get; set; } = new();

        // ===== TESTS (Lý thuyết + Mô phỏng) =====
        public TestsSectionVM Tests { get; set; } = new();

        public ReportExportConfirmVM ExportConfirm { get; set; } = new();

    }
}
