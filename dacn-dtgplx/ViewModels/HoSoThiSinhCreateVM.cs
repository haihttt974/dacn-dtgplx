using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace dacn_dtgplx.ViewModels
{
    public class HoSoThiSinhCreateVM
    {
        [Required]
        [Display(Name = "Hạng GPLX")]
        public int IdHang { get; set; }

        [Required]
        [Display(Name = "Ảnh thẻ")]
        public IFormFile AnhThe { get; set; } = null!;

        [Required]
        [Display(Name = "Ảnh giấy khám sức khỏe")]
        public List<IFormFile> AnhGiayKham { get; set; } = new();

        [Required]
        public HealthInputVM SucKhoe { get; set; } = new();
    }
}
