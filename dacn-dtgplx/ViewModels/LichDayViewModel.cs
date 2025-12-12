using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class LichDayViewModel
    {
        public TtGiaoVien GiaoVien { get; set; }

        public string Mode { get; set; } = "day";

        public DateTime CurrentDate { get; set; } = DateTime.Today;

        public List<LichDayItem> LichDayItems { get; set; } = new();

        // Tuần đang chọn
        public int? SelectedWeek { get; set; }

        // Tháng & năm đang chọn
        public int SelectedMonth { get; set; } = DateTime.Today.Month;
        public int SelectedYear { get; set; } = DateTime.Today.Year;

        // Danh sách tuần (đổ ra dropdown)
        public List<WeekItem> Weeks { get; set; } = new();
    }

    // Tuần với thời gian bắt đầu → kết thúc
    public class WeekItem
    {
        public int WeekIndex { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Hiển thị dạng: "01/12/2025 - 07/12/2025"
        public string DisplayName => $"{Start:dd/MM/yyyy} - {End:dd/MM/yyyy}";
        public string Label => DisplayName;
    }
}
