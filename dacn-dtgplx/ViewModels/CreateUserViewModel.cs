using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace dacn_dtgplx.ViewModels
{
    public class CreateUserViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Họ và tên")]
        public string? TenDayDu { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Vai trò")]
        public int? RoleId { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool TrangThai { get; set; } = true;

        // Mật khẩu sẽ được random từ UI – để readonly trong View
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        // Upload avatar
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }
    }
}
