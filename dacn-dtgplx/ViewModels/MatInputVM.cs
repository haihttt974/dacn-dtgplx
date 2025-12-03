using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.ViewModels
{
    public class MatInputVM
    {
        [Required]
        [Display(Name = "Mắt trái (10/10)")]
        public int? mat_trai { get; set; }

        [Required]
        [Display(Name = "Mắt phải (10/10)")]
        public int? mat_phai { get; set; }
    }
}
