namespace dacn_dtgplx.ViewModels.Reports
{
    public class ReportFilterVM
    {
        public DateTime? FromDate { get; set; }                           // lọc từ ngày
        public DateTime? ToDate { get; set; }                             // lọc đến ngày

        // tuỳ chọn mở rộng (sau này muốn lọc theo loại doanh thu, role, hạng…)
        public int? HangId { get; set; }
        public bool? OnlySuccessfulPayments { get; set; }                 // chỉ tính hóa đơn TrangThai == true?
    }
}
