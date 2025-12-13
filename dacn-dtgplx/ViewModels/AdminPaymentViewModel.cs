namespace dacn_dtgplx.ViewModels
{
    public class AdminPaymentViewModel
    {
        public int IdThanhToan { get; set; }

        public string? TenHocVien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }

        public string? TenKhoaHoc { get; set; }

        public decimal? SoTien { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public DateOnly? NgayThanhToan { get; set; }

        public bool? TrangThai { get; set; }
    }
}
