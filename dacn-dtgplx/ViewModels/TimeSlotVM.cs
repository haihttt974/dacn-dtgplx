namespace dacn_dtgplx.ViewModels
{
    public class TimeSlotVM
    {
        public int Index { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public string Label { get; set; } = "";
    }
}
