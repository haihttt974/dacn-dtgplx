using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace dacn_dtgplx.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClient;

        public PayPalService(IConfiguration config, IHttpClientFactory httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        private string GetBaseUrl()
        {
            string mode = _config["PayPal:Mode"] ?? "sandbox";
            return mode == "live" ?
                "https://api-m.paypal.com" :
                "https://api-m.sandbox.paypal.com";
        }

        private async Task<string> GetAccessToken()
        {
            var clientId = _config["PayPal:ClientId"];
            var secret = _config["PayPal:ClientSecret"];

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", auth);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{GetBaseUrl()}/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                })
            };

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayPal Auth Error: {content}");

            dynamic json = JsonConvert.DeserializeObject(content);
            return json.access_token.ToString();
        }

        // ============================================================
        // 1) CREATE ORDER
        // ============================================================
        public async Task<string?> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
        {
            var token = await GetAccessToken();

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

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
                            value = amount.ToString("F2")
                        }
                    }
                },
                application_context = new
                {
                    brand_name = "Online Course Payment",
                    landing_page = "LOGIN",
                    user_action = "PAY_NOW",
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{GetBaseUrl()}/v2/checkout/orders", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("CreateOrder Error: " + result);

            dynamic data = JsonConvert.DeserializeObject(result);

            foreach (var link in data.links)
            {
                if (link.rel == "approve")
                    return link.href.ToString();
            }

            return null;
        }

        // ============================================================
        // 2) CAPTURE ORDER
        // ============================================================
        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessToken();

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(
                $"{GetBaseUrl()}/v2/checkout/orders/{orderId}/capture",
                new StringContent("{}", Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return false;

            dynamic data = JsonConvert.DeserializeObject(json);
            return data.status == "COMPLETED";
        }
    }
}
