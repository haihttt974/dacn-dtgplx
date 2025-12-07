using dacn_dtgplx.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace dacn_dtgplx.Services
{
    public class MomoService : IMomoService
    {
        private readonly DtGplxContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _http;

        public MomoService(DtGplxContext context, IConfiguration config, IHttpClientFactory http)
        {
            _context = context;
            _config = config;
            _http = http;
        }

        public async Task<string> CreatePaymentUrl(
            HoaDonThanhToan hoaDon,
            string returnUrl,
            string ipnUrl,
            string fakeReturnUrl)
        {
            bool sandbox = _config.GetValue<bool>("MOMO:UseSandbox");

            // -------------------------
            // 1️⃣ SANDBOX MODE → FAKE PAGE
            // -------------------------
            if (sandbox)
                return fakeReturnUrl;

            // -------------------------
            // 2️⃣ REAL MOMO PAYMENT
            // -------------------------
            string endpoint = _config["MOMO:Endpoint"]!;
            string partnerCode = _config["MOMO:PartnerCode"]!;
            string accessKey = _config["MOMO:AccessKey"]!;
            string secretKey = _config["MOMO:SecretKey"]!;

            long amount = (long)(hoaDon.SoTien ?? 0);

            string orderId = "ORDER" + DateTime.Now.Ticks;
            string requestId = Guid.NewGuid().ToString();
            string orderInfo = $"Thanh toán khóa học {hoaDon.IdDangKyNavigation.KhoaHoc.TenKhoaHoc}";
            string extraData = hoaDon.IdThanhToan.ToString();

            string requestType = "captureWallet";

            string raw =
                $"accessKey={accessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={ipnUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={partnerCode}" +
                $"&redirectUrl={returnUrl}" +
                $"&requestId={requestId}" +
                $"&requestType={requestType}";

            string signature = HmacSHA256(raw, secretKey);

            var payload = new
            {
                partnerCode,
                partnerName = "GPLX Center",
                storeId = "GPLX-STORE",
                requestId,
                orderId,
                amount,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                lang = "vi",
                requestType,
                autoCapture = true,
                extraData,
                signature
            };

            var client = _http.CreateClient();

            var response = await client.PostAsync(
                $"{endpoint}/v2/gateway/api/create",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            string json = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json);

            if (data.resultCode != 0)
                throw new Exception($"MoMo error: {data.message}");

            return data.payUrl;
        }

        public async Task<MomoResult> ProcessReturn(IQueryCollection query)
        {
            string partnerCode = query["partnerCode"];
            string orderId = query["orderId"];
            string requestId = query["requestId"];
            string amount = query["amount"];
            string orderInfo = query["orderInfo"];
            string orderType = query["orderType"];
            string transId = query["transId"];
            string resultCode = query["resultCode"];
            string message = query["message"];
            string payType = query["payType"];
            string responseTime = query["responseTime"];
            string extraData = query["extraData"];
            string signature = query["signature"];

            string accessKey = _config["MOMO:AccessKey"]!;
            string secretKey = _config["MOMO:SecretKey"]!;

            string raw =
                $"accessKey={accessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&message={message}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&orderType={orderType}" +
                $"&partnerCode={partnerCode}" +
                $"&payType={payType}" +
                $"&requestId={requestId}" +
                $"&responseTime={responseTime}" +
                $"&resultCode={resultCode}" +
                $"&transId={transId}";

            string computed = HmacSHA256(raw, secretKey);

            if (computed != signature)
            {
                return new MomoResult
                {
                    Success = false,
                    Message = "Sai chữ ký MoMo"
                };
            }

            if (resultCode != "0")
            {
                return new MomoResult
                {
                    Success = false,
                    Message = "Thanh toán thất bại"
                };
            }

            int hoaDonId = int.Parse(extraData);

            var hd = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null)
                return new MomoResult { Success = false, Message = "Không tìm thấy hóa đơn" };

            // -------------------------
            // UPDATE HÓA ĐƠN
            // -------------------------
            hd.TrangThai = true;
            hd.NgayThanhToan = DateOnly.FromDateTime(DateTime.Now);
            hd.IdDangKyNavigation.TrangThai = true;

            await _context.SaveChangesAsync();

            return new MomoResult
            {
                Success = true,
                HoaDonId = hoaDonId
            };
        }

        private string HmacSHA256(string input, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(input))
            ).Replace("-", "").ToLower();
        }
    }
}
