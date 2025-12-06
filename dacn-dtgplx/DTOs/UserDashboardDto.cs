namespace dacn_dtgplx.DTOs
{
    public class UserDashboardDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        public HoSoDashboardDto? HoSo { get; set; }
        public TienDoDashboardDto? TienDo { get; set; }
        public List<LichHocDashboardDto> LichHoc { get; set; } = new();
    }
}
