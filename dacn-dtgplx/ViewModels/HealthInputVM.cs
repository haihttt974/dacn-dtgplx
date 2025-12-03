using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.ViewModels
{
    public class HealthInputVM
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Thời hạn giấy khám")]
        public DateTime? thoi_han { get; set; }

        [Required]
        public MatInputVM mat { get; set; } = new();

        [Required]
        [Display(Name = "Huyết áp")]
        public string huyet_ap { get; set; } = null!;

        [Required]
        [Display(Name = "Chiều cao (cm)")]
        public int? chieu_cao { get; set; }

        [Required]
        [Display(Name = "Cân nặng (kg)")]
        public int? can_nang { get; set; }
    }
}
