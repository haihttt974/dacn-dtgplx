using System.Threading.Tasks;

namespace dacn_dtgplx.Payments
{
    public interface IPayPalService
    {
        Task<string?> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl);
        Task<bool> CaptureOrderAsync(string token);
    }
}
