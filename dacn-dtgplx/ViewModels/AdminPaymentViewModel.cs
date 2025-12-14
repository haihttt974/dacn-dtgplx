namespace dacn_dtgplx.ViewModels
{
    public class AdminPaymentViewModel
    {
        public int IdThanhToan { get; set; }

        // Người thanh toán
        public string? TenNguoiThanhToan { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }

        // Loại thanh toán
        public string LoaiThanhToan { get; set; } = null!; // "Khóa học" | "Thuê xe"

        // Thông tin chi tiết
        public string? TenKhoaHoc { get; set; }
        public string? XeTapLai { get; set; }

        public decimal? SoTien { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public DateOnly? NgayThanhToan { get; set; }

        public bool? TrangThai { get; set; }
    }
}
