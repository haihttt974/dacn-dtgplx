namespace dacn_dtgplx.ViewModels
{
    public class VehicleScheduleItemVM
    {
        public DateOnly Date { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }

        // "LichHoc" hoặc "PhieuThue"
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
    }
}
