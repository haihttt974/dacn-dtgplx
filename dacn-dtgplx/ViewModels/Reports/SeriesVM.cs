namespace dacn_dtgplx.ViewModels.Reports
{
    public class SeriesVM
    {
        public string Name { get; set; } = "";                            // "Doanh thu khóa học"
        public List<decimal> Data { get; set; } = new();                  // cùng số phần tử với Labels
    }
}
