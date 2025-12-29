using System.ComponentModel.DataAnnotations;

public class ContactViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(150)]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung liên hệ")]
    [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
    public string Message { get; set; }
}
