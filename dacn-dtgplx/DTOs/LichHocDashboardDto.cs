namespace dacn_dtgplx.DTOs
{
    public class LichHocDashboardDto
    {
        public DateOnly NgayHoc { get; set; }   // dùng DateOnly luôn cho khỏe
        public string? NoiDung { get; set; }
        public string? TenKhoaHoc { get; set; }
        public string? LoaiXe { get; set; }
        public string? DiaDiem { get; set; }
    }
}
