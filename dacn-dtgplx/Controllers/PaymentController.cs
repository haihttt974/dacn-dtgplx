using CinemaS.VNPAY;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace dacn_dtgplx.Controllers
{
    [Authorize]
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly IConfiguration _config;
        private readonly IMailService _mail;
        private readonly IPayPalService _payPal;
        public PaymentController(DtGplxContext context, IConfiguration config, IMailService mail, IPayPalService payPal)
        {
            _context = context;
            _config = config;
            _mail = mail;
            _payPal = payPal;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        // ============================================================
        // 1) TRANG BẮT ĐẦU THANH TOÁN
        // ============================================================
        [HttpGet("start")]
        public async Task<IActionResult> StartPayment(int hoaDonId)
        {
            var hoaDon = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(d => d.KhoaHoc)
                        .ThenInclude(k => k.IdHangNavigation)
                .Include(h => h.IdDangKyNavigation.HoSo)
                    .ThenInclude(hs => hs.User)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hoaDon == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn thanh toán.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            return View("StartPayment", hoaDon);
        }


        // ============================================================
        // 2) CHỌN PHƯƠNG THỨC THANH TOÁN
        // ============================================================
        [HttpPost("choose")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChoosePaymentMethod(int hoaDonId, string method, string noiDung)
        {
            var hoaDon = await _context.HoaDonThanhToans
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hoaDon == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            // lưu nội dung + phương thức
            hoaDon.NoiDung = noiDung;
            hoaDon.PhuongThucThanhToan = method;
            await _context.SaveChangesAsync();

            if (method == "VNPAY")
                return RedirectToAction("VnPay", new { hoaDonId });

            if (method == "PAYPAL")
                return RedirectToAction("PayPal", new { hoaDonId });

            TempData["Error"] = "Phương thức thanh toán chưa được hỗ trợ.";
            return RedirectToAction("StartPayment", new { hoaDonId });
        }


        // ============================================================
        // 3) TẠO URL THANH TOÁN VNPAY
        // ============================================================
        [HttpGet("vnpay")]
        public async Task<IActionResult> VnPay(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(d => d.KhoaHoc)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            var amountDecimal = hd.SoTien ?? 0;
            if (amountDecimal <= 0)
            {
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            long amount = (long)amountDecimal;

            string baseUrl = _config["VnPay:BaseUrl"]!;
            string tmnCode = _config["VnPay:TmnCode"]!;
            string hashSecret = _config["VnPay:HashSecret"]!;
            string orderType = _config["VnPay:OrderType"] ?? "other";
            string locale = _config["VnPay:Locale"] ?? "vn";
            string currCode = _config["VnPay:CurrCode"] ?? "VND";

            string returnUrl = Url.Action("VnPayReturn", "Payment", null, Request.Scheme)!;

            var vnp = new VnPayLibrary();
            vnp.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", tmnCode);
            vnp.AddRequestData("vnp_Amount", (amount * 100).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", currCode);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";
            vnp.AddRequestData("vnp_IpAddr", ip);

            vnp.AddRequestData("vnp_Locale", locale);

            string infoRaw = string.IsNullOrWhiteSpace(hd.NoiDung)
                ? $"Thanh toan khoa hoc {hd.IdDangKyNavigation?.KhoaHoc?.TenKhoaHoc}"
                : hd.NoiDung;
            vnp.AddRequestData("vnp_OrderInfo", RemoveVietnameseSigns(infoRaw));

            vnp.AddRequestData("vnp_OrderType", orderType);
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);

            // 🔥 TxnRef: chứa luôn id hóa đơn + timestamp, đảm bảo duy nhất, <= 20 ký tự
            string txnRef = $"{hoaDonId}-{DateTime.Now:yyyyMMddHHmmss}";
            vnp.AddRequestData("vnp_TxnRef", txnRef);

            string paymentUrl = vnp.CreateRequestUrl(baseUrl, hashSecret);
            return Redirect(paymentUrl);
        }

        // ============================================================
        // 4) RETURN TỪ VNPAY
        // ============================================================
        [AllowAnonymous]
        [HttpGet("vnpayreturn")]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnp = new VnPayLibrary();
            foreach (var key in Request.Query.Keys)
                vnp.AddResponseData(key, Request.Query[key]);

            string secureHash = Request.Query["vnp_SecureHash"];
            bool isValid = vnp.ValidateSignature(secureHash, _config["VnPay:HashSecret"]);

            if (!isValid)
            {
                ViewBag.Message = "Chữ ký VNPAY không hợp lệ.";
                return View("PaymentFail");
            }

            string txnRef = vnp.GetResponseData("vnp_TxnRef");
            // txnRef: "2-20251205115718" -> lấy phần trước dấu '-'
            int hoaDonId = int.Parse(txnRef.Split('-')[0]);

            string responseCode = vnp.GetResponseData("vnp_ResponseCode");

            var hd = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                    .ThenInclude(d => d.HoSo)
                        .ThenInclude(hs => hs.User)
                .Include(h => h.IdDangKyNavigation.KhoaHoc)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null)
            {
                ViewBag.Message = "Không tìm thấy hóa đơn khi VNPAY trả về.";
                return View("PaymentFail");
            }

            var dk = hd.IdDangKyNavigation;
            var user = dk.HoSo.User;

            if (responseCode == "00")
            {
                hd.TrangThai = true;
                hd.NgayThanhToan = DateOnly.FromDateTime(DateTime.Now);
                dk.TrangThai = true;
                await _context.SaveChangesAsync();

                //  TẠO THÔNG BÁO CHO USER
                var thongBao = new ThongBao
                {
                    TieuDe = "Thanh toán thành công",
                    NoiDung = $"Bạn đã thanh toán thành công {dk.KhoaHoc.TenKhoaHoc}.",
                    TaoLuc = DateTime.Now,
                    SendRole = null
                };
                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();
                var ct = new CtThongBao
                {
                    UserId = user.UserId,
                    ThongBaoId = thongBao.ThongBaoId,
                    ThoiGianGui = DateTime.Now,
                    DaXem = false
                };
                _context.CtThongBaos.Add(ct);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thanh toán thành công!";

                await _mail.SendPaymentSuccessEmail(
                    user.Email!,
                    user.TenDayDu ?? user.Username,
                    dk.KhoaHoc.TenKhoaHoc!,
                    hd.SoTien ?? 0
                );

                return View("PaymentSuccess");
            }
            else
            {
                hd.TrangThai = false;
                dk.TrangThai = false;
                await _context.SaveChangesAsync();

                ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                return View("PaymentFail");
            }
        }

        // ============================================================
        // Helper: bỏ dấu tiếng Việt để gửi sang VNPAY
        // ============================================================
        private static string RemoveVietnameseSigns(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);

            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "D");
        }

        // ============================================================
        // PAYPAL
        // ============================================================
        [HttpGet("paypal")]
        public async Task<IActionResult> PayPal(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                .ThenInclude(d => d.KhoaHoc)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            decimal vndAmount = hd.SoTien ?? 0;
            if (vndAmount <= 0)
            {
                TempData["Error"] = "Số tiền không hợp lệ.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            decimal usd = Math.Round(vndAmount / 24000, 2);
            if (usd < 0.01m) usd = 0.01m;

            string returnUrl = Url.Action("PayPalReturn", "Payment", new { hoaDonId }, Request.Scheme)!;
            string cancelUrl = Url.Action("PayPalCancel", "Payment", new { hoaDonId }, Request.Scheme)!;

            string? approval = await _payPal.CreateOrderAsync(usd, "USD", returnUrl, cancelUrl);

            if (approval == null)
            {
                TempData["Error"] = "Không tạo được đơn PayPal.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            return Redirect(approval);
        }

        [AllowAnonymous]
        [HttpGet("paypalreturn")]
        public async Task<IActionResult> PayPalReturn(int hoaDonId, string token)
        {
            bool success = await _payPal.CaptureOrderAsync(token);

            var hd = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                .ThenInclude(d => d.HoSo)
                    .ThenInclude(hs => hs.User)
                .Include(h => h.IdDangKyNavigation.KhoaHoc)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null)
            {
                ViewBag.Message = "Không tìm thấy hóa đơn.";
                return View("PaymentFail");
            }

            var dk = hd.IdDangKyNavigation;
            var user = dk.HoSo.User;

            if (success)
            {
                hd.TrangThai = true;
                hd.NgayThanhToan = DateOnly.FromDateTime(DateTime.Now);
                dk.TrangThai = true;

                await _context.SaveChangesAsync();

                await _mail.SendPaymentSuccessEmail(
                    user.Email!,
                    user.TenDayDu ?? user.Username,
                    dk.KhoaHoc.TenKhoaHoc!,
                    hd.SoTien ?? 0
                );

                return View("PaymentSuccess");
            }

            hd.TrangThai = false;
            dk.TrangThai = false;
            await _context.SaveChangesAsync();

            ViewBag.Message = "Thanh toán PayPal thất bại hoặc bị hủy.";
            return View("PaymentFail");
        }

        [AllowAnonymous]
        [HttpGet("paypalcancel")]
        public async Task<IActionResult> PayPalCancel(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.IdDangKyNavigation)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd != null)
            {
                hd.TrangThai = false;
                hd.IdDangKyNavigation.TrangThai = false;
                await _context.SaveChangesAsync();
            }

            ViewBag.Message = "Bạn đã hủy thanh toán PayPal.";
            return View("PaymentFail");
        }
    }
}
