using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class AdminHoSoDetailVM
    {
        public HoSoThiSinh HoSo { get; set; }
        public HealthInfoVM? SucKhoe { get; set; }       // object đã parse từ JSON
        public string? RawJson { get; set; }             // JSON thô
        public List<string> DanhSachAnhGiayKham { get; set; } = new();
    }
}
