using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? TenDayDu { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "CCCD / CMND")]
        public string? Cccd { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? Avatar { get; set; }
    }
}
