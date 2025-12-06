using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.ViewModels
{
    public class UserSettingsViewModel
    {
        // -------- THÔNG TIN TÀI KHOẢN --------
        public int UserId { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Tên tài khoản")]
        public string Username { get; set; }

        [Display(Name = "Họ và tên")]
        public string TenDayDu { get; set; }

        [Display(Name = "Vai trò")]
        public string RoleName { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime TaoLuc { get; set; }

        [Display(Name = "Đăng nhập gần nhất")]
        public DateTime? LanDangNhapGanNhat { get; set; }


        // -------- CÀI ĐẶT BẢO MẬT --------
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; }


        // -------- CÀI ĐẶT RIÊNG TƯ / THÔNG BÁO --------
        [Display(Name = "Cho phép hiển thị hồ sơ cho giáo viên")]
        public bool AllowProfileVisibility { get; set; }

        [Display(Name = "Nhận thông báo email")]
        public bool AllowEmailNotifications { get; set; }

        [Display(Name = "Nhận thông báo lịch học / lịch thi")]
        public bool AllowScheduleNotifications { get; set; }


        // -------- DANGER ZONE --------
        [Display(Name = "Xác nhận xóa tài khoản")]
        public string DeleteConfirmText { get; set; }
    }
}
