namespace dacn_dtgplx.ViewModels
{
    public class VehicleScheduleVM
    {
        public int XeId { get; set; }
        public string LoaiXe { get; set; } = "";
        public bool TrangThaiXe { get; set; }

        // "week" hoặc "month"
        public string Mode { get; set; } = "week";

        // WEEK VIEW
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DateTime> WeekDays { get; set; } = new();
        public List<TimeSlotVM> TimeSlots { get; set; } = new();
        public List<VehicleScheduleItemVM> Items { get; set; } = new();

        // MONTH VIEW
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public List<VehicleScheduleItemVM> MonthItems { get; set; } = new();
    }
}
