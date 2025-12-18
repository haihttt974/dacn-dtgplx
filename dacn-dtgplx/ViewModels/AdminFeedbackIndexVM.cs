namespace dacn_dtgplx.ViewModels
{
    public class AdminFeedbackIndexVM
    {
        public List<AdminFeedbackItem> Items { get; set; } = new();
        public decimal AvgRating { get; set; }
        public int TotalCount { get; set; }
    }
}
