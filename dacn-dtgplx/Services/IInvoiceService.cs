using dacn_dtgplx.Models;

namespace dacn_dtgplx.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(HoaDonThanhToan bill);
    }
}
