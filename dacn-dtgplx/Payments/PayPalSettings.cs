namespace dacn_dtgplx.Payments
{
    public class PayPalSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Business { get; set; } = string.Empty;
        public string Mode { get; set; } = "sandbox"; // "live" khi lên production
    }
}
