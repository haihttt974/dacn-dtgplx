using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace dacn_dtgplx.Payments
{
    public class PayPalService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly PayPalSettings _settings;

        public PayPalService(IOptions<PayPalSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient();

            // BaseUrl theo mode
            var baseUrl = _settings.Mode == "live"
                ? "https://api-m.paypal.com"
                : "https://api-m.sandbox.paypal.com";

            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var authToken = Encoding.ASCII.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}");
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            return token!;
        }

        public async Task<string?> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                },
                application_context = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v2/checkout/orders", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // TODO: log lỗi responseString
                return null;
            }

            using var doc = JsonDocument.Parse(responseString);
            // Tìm link có rel = "approve"
            var links = doc.RootElement.GetProperty("links").EnumerateArray();
            foreach (var link in links)
            {
                var rel = link.GetProperty("rel").GetString();
                if (rel == "approve")
                {
                    return link.GetProperty("href").GetString();
                }
            }

            return null;
        }

        public async Task<bool> CaptureOrderAsync(string token)
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync($"/v2/checkout/orders/{token}/capture", null);
            var responseString = await response.Content.ReadAsStringAsync();

            // ❗ THÊM LOG TẠI ĐÂY
            Console.WriteLine("===== PAYPAL CAPTURE RESPONSE =====");
            Console.WriteLine(responseString);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("StatusCode: " + response.StatusCode);
                return false;
            }

            using var doc = JsonDocument.Parse(responseString);
            var status = doc.RootElement.GetProperty("status").GetString();

            return status == "COMPLETED";
        }
    }
}
