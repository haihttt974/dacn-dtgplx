using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Http;

namespace dacn_dtgplx.Services
{
    public interface IMomoService
    {
        Task<string> CreatePaymentUrl(
            HoaDonThanhToan hoaDon,
            string returnUrl,
            string ipnUrl,
            string fakeReturnUrl);

        Task<MomoResult> ProcessReturn(IQueryCollection query);
    }

    public class MomoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int? HoaDonId { get; set; }
    }
}
