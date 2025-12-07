namespace dacn_dtgplx.Services
{
    public interface IPayPalService
    {
        Task<string?> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl);
        Task<bool> CaptureOrderAsync(string orderId);
    }
}
