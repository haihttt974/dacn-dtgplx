using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.ViewModels
{
    public class MatInputVM
    {
        [Required]
        [Display(Name = "Mắt trái")]
        [RegularExpression(@"^(10|[1-9])\/10$",
        ErrorMessage = "Thị lực phải theo định dạng X/10 (VD: 8/10, 10/10)")]
        public string? mat_trai { get; set; }

        [Required]
        [Display(Name = "Mắt phải")]
        [RegularExpression(@"^(10|[1-9])\/10$",
        ErrorMessage = "Thị lực phải theo định dạng X/10 (VD: 8/10, 10/10)")]
        public string? mat_phai { get; set; }
    }
}
