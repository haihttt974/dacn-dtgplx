namespace dacn_dtgplx.ViewModels
{
    public class AdminFeedbackItem
    {
        public int PhanHoiId { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianPh { get; set; }
        public decimal SoSao { get; set; }

        // Người gửi
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
