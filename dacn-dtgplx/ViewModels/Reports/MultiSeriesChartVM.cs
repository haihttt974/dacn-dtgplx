namespace dacn_dtgplx.ViewModels.Reports
{
    /// <summary>
    /// Dùng cho biểu đồ nhiều chuỗi trên cùng 1 trục (ví dụ: Doanh thu KH vs Xe)
    /// </summary>
    public class MultiSeriesChartVM
    {
        public string Title { get; set; } = "";
        public List<string> Labels { get; set; } = new();                 // ["1/2025","2/2025",...]
        public List<SeriesVM> Series { get; set; } = new();               // nhiều đường/cột
    }
}
