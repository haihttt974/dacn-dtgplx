using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class DetailHoSoVM
    {
        public HoSoThiSinh HoSo { get; set; } = null!;
        public HealthInfoVM? SucKhoe { get; set; }
        public string? RawJson { get; set; }
    }
}
