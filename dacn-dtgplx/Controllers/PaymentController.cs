using System.Security.Claims;
using CinemaS.VNPAY;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    [Authorize]
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly IConfiguration _config;

        public PaymentController(DtGplxContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ============================
        // 1) START PAYMENT PAGE
        // ============================
        [HttpGet("start")]
        public async Task<IActionResult> StartPayment(int dangKyId)
        {
            int userId = GetUserId();

            var dk = await _context.DangKyHocs
                .Include(d => d.KhoaHoc)
                    .ThenInclude(k => k.IdHangNavigation)
                .Include(d => d.HoSo)
                .FirstOrDefaultAsync(d => d.IdDangKy == dangKyId);

            if (dk == null || dk.HoSo.UserId != userId)
            {
                TempData["Error"] = "Không tìm thấy thông tin đăng ký.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            ViewBag.SoTien = dk.KhoaHoc!.IdHangNavigation!.ChiPhi ?? 0;

            return View("StartPayment", dk);
        }

        // ============================
        // 2) VNPAY – CREATE PAYMENT URL
        // ============================
        [HttpPost("vnpay")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VnPay(int dangKyId)
        {
            int userId = GetUserId();

            var dk = await _context.DangKyHocs
                .Include(d => d.KhoaHoc)
                    .ThenInclude(k => k.IdHangNavigation)
                .Include(d => d.HoSo)
                .FirstOrDefaultAsync(d => d.IdDangKy == dangKyId);

            if (dk == null || dk.HoSo == null || dk.HoSo.UserId != userId)
            {
                TempData["Error"] = "Không tìm thấy thông tin đăng ký.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            long amount = (long)(dk.KhoaHoc.IdHangNavigation!.ChiPhi ?? 0);

            if (amount <= 0)
            {
                TempData["Error"] = "Học phí không hợp lệ.";
                return RedirectToAction("StartPayment", new { dangKyId });
            }

            string vnp_Url = _config["VnPay:BaseUrl"]!;
            string vnp_TmnCode = _config["VnPay:TmnCode"]!;
            string vnp_HashSecret = _config["VnPay:HashSecret"]!;

            string vnp_ReturnUrl = $"{Request.Scheme}://{Request.Host}/payment/vnpayreturn";

            var vnp = new VnPayLibrary();
            vnp.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnp.AddRequestData("vnp_Amount", (amount * 100).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", "VND");

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

            vnp.AddRequestData("vnp_IpAddr", ip);
            vnp.AddRequestData("vnp_Locale", "vn");

            // ⭐ Không Unicode
            vnp.AddRequestData("vnp_OrderInfo", $"Thanh toan dang ky {dk.IdDangKy}");

            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);

            // ⭐ TxnRef phải không ký tự đặc biệt
            vnp.AddRequestData("vnp_TxnRef", dk.IdDangKy.ToString());

            string paymentUrl = vnp.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            Console.WriteLine("=== FULL VNPAY URL ===");
            Console.WriteLine(paymentUrl);

            return Redirect(paymentUrl);
        }

        // ============================
        // 3) VNPAY RETURN
        // ============================
        [AllowAnonymous]
        [HttpGet("vnpayreturn")]
        public IActionResult VnPayReturn()
        {
            try
            {
                var vnpData = Request.Query;
                if (!vnpData.Any())
                {
                    ViewBag.Message = "Không nhận được dữ liệu từ VNPAY.";
                    return View("PaymentFail");
                }

                var vnp = new VnPayLibrary();

                // Chỉ lấy các param bắt đầu bằng vnp_
                foreach (var kv in vnpData)
                {
                    if (!string.IsNullOrEmpty(kv.Key) && kv.Key.StartsWith("vnp_"))
                    {
                        vnp.AddResponseData(kv.Key, kv.Value);
                    }
                }

                var vnp_HashSecret = _config["VnPay:HashSecret"];
                var vnp_SecureHash = vnpData["vnp_SecureHash"].ToString();

                if (string.IsNullOrEmpty(vnp_HashSecret) || string.IsNullOrEmpty(vnp_SecureHash))
                {
                    ViewBag.Message = "Thiếu thông tin chữ ký từ VNPAY.";
                    return View("PaymentFail");
                }

                // Kiểm tra chữ ký
                bool validSignature = vnp.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (!validSignature)
                {
                    ViewBag.Message = "Xác thực chữ ký không hợp lệ!";
                    return View("PaymentFail");
                }

                string orderId = vnp.GetResponseData("vnp_TxnRef");
                string responseCode = vnp.GetResponseData("vnp_ResponseCode");

                if (responseCode == "00")
                {
                    // TODO: update trạng thái đơn/đăng ký trong DB ở đây nếu cần
                    ViewBag.OrderId = orderId;
                    return View("PaymentSuccess");
                }
                else
                {
                    ViewBag.OrderId = orderId;
                    ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                    return View("PaymentFail");
                }
            }
            catch (Exception ex)
            {
                // Log ex nếu muốn
                ViewBag.Message = "Có lỗi xảy ra khi xử lý kết quả thanh toán: " + ex.Message;
                return View("PaymentFail");
            }
        }
    }
}
