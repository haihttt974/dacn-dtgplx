using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class PaymentHistoryVM
    {
        public List<HoaDonThanhToan> PaymentsKhoaHoc { get; set; } = new();
        public List<HoaDonThanhToan> PaymentsThueXe { get; set; } = new();
    }
}
