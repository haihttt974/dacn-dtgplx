using System.ComponentModel.DataAnnotations;

namespace dacn_dtgplx.ViewModels
{
    public class HealthInputVM : IValidatableObject
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Thời hạn giấy khám")]
        public DateTime? thoi_han { get; set; }

        [Required]
        public MatInputVM mat { get; set; } = new();

        [Required]
        [Display(Name = "Huyết áp")]
        [RegularExpression(
            @"^(9[0-9]|1[0-9]{2}|200)\/(6[0-9]|7[0-9]|8[0-9]|90)$",
            ErrorMessage = "Huyết áp không hợp lệ (VD: 120/80)"
        )]
        public string huyet_ap { get; set; } = null!;

        [Required]
        [Display(Name = "Chiều cao (cm)")]
        public int? chieu_cao { get; set; }

        [Required]
        [Display(Name = "Cân nặng (kg)")]
        public int? can_nang { get; set; }

        // ===============================
        // VALIDATE NGÀY KHÁM
        // ===============================
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (thoi_han == null)
                yield break;

            DateTime today = DateTime.Today;
            DateTime minDate = today.AddMonths(-12);

            // ❌ Không được là ngày tương lai
            if (thoi_han.Value.Date > today)
            {
                yield return new ValidationResult(
                    "Ngày khám không được lớn hơn ngày hiện tại.",
                    new[] { nameof(thoi_han) }
                );
            }

            // ❌ Không được quá 12 tháng
            if (thoi_han.Value.Date < minDate)
            {
                yield return new ValidationResult(
                    "Giấy khám sức khỏe chỉ có giá trị trong vòng 12 tháng.",
                    new[] { nameof(thoi_han) }
                );
            }
        }
    }
}
