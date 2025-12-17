namespace dacn_dtgplx.ViewModels.Reports
{
    /// <summary>
    /// Card KPI hiển thị dạng “web chuyên nghiệp”: title + value + subtitle + delta
    /// </summary>
    public class KpiCardVM
    {
        public string Title { get; set; } = "";                           // "Tổng doanh thu"
        public string ValueText { get; set; } = "";                       // "120,000,000"
        public string Unit { get; set; } = "";                            // "VND", "lượt", "HV"
        public string? SubTitle { get; set; }                             // "Trong khoảng thời gian lọc"
        public decimal? DeltaPercent { get; set; }                        // % tăng/giảm so với kỳ trước (nếu có)
        public string? DeltaHint { get; set; }                            // "so với kỳ trước"
        public string? IconCss { get; set; }                              // tùy UI (FontAwesome)
    }
}
