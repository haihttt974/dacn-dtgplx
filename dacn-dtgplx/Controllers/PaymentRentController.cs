using CinemaS.VNPAY;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace dacn_dtgplx.Controllers
{
    // Thuê xe cho phép cả khách vãng lai, nên để AllowAnonymous
    [AllowAnonymous]
    [Route("PaymentRent")]
    public class PaymentRentController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly IConfiguration _config;
        private readonly IMailService _mail;
        private readonly IPayPalService _payPal;
        private readonly IMomoService _momo;
        private readonly QrCryptoService _qrCryptoService;
        private readonly IWebHostEnvironment _env;

        public PaymentRentController(
            DtGplxContext context,
            IConfiguration config,
            IMailService mail,
            IPayPalService payPal,
            IMomoService momo,
            QrCryptoService qrCryptoService,
            IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _mail = mail;
            _payPal = payPal;
            _momo = momo;
            _qrCryptoService = qrCryptoService;
            _env = env;
        }

        private int? GetUserId()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return null;
        }

        // ============================================================
        // 1) TRANG BẮT ĐẦU THANH TOÁN THUÊ XE
        // ============================================================
        [HttpGet("start")]
        public async Task<IActionResult> StartPayment(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTxId == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn thuê xe.";
                return RedirectToAction("Index", "ThueXe");
            }

            return View("StartPaymentRent", hd);
        }

        // ============================================================
        // 2) CHỌN PHƯƠNG THỨC THANH TOÁN
        // ============================================================
        [HttpPost("ChooseMethod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChoosePaymentMethod(int hoaDonId, string method, string noiDung)
        {
            var hd = await _context.HoaDonThanhToans
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "ThueXe");
            }

            hd.PhuongThucThanhToan = method;
            hd.NoiDung = noiDung;
            await _context.SaveChangesAsync();

            return method?.ToUpperInvariant() switch
            {
                "VNPAY" => RedirectToAction("VnPay", new { hoaDonId }),
                "PAYPAL" => RedirectToAction("PayPal", new { hoaDonId }),
                "MOMO" => RedirectToAction("MoMo", new { hoaDonId }),
                _ => RedirectToAction("StartPayment", new { hoaDonId })
            };
        }

        // ============================================================
        // 3) VNPAY
        // ============================================================
        [HttpGet("vnpay")]
        public async Task<IActionResult> VnPay(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTx == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn thuê xe.";
                return RedirectToAction("Index", "ThueXe");
            }

            var soTien = hd.SoTien ?? 0;
            if (soTien <= 0)
            {
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            var amount = (long)soTien;

            // Lấy config giống bên PaymentController
            string baseUrl = _config["VnPay:BaseUrl"] ?? "";
            string tmnCode = _config["VnPay:TmnCode"] ?? "";
            string hashSecret = _config["VnPay:HashSecret"] ?? "";
            string orderType = _config["VnPay:OrderType"] ?? "other";
            string locale = _config["VnPay:Locale"] ?? "vn";
            string currCode = _config["VnPay:CurrCode"] ?? "VND";

            // URL callback
            string returnUrl = Url.Action("VnPayReturn", "PaymentRent", null, Request.Scheme)!;

            var vnp = new VnPayLibrary();
            vnp.AddRequestData("vnp_Version", "2.1.0");
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", tmnCode);
            vnp.AddRequestData("vnp_Amount", (amount * 100).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", currCode);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";
            vnp.AddRequestData("vnp_IpAddr", ip);

            vnp.AddRequestData("vnp_Locale", locale);

            // Bỏ dấu tiếng Việt trong OrderInfo
            //string infoRaw = string.IsNullOrWhiteSpace(hd.NoiDung)
            //    ? $"Thanh toan thue xe {hd.PhieuTx.Xe.LoaiXe}"
            //    : hd.NoiDung;
            //vnp.AddRequestData("vnp_OrderInfo", RemoveVietnamese(infoRaw));


            string orderInfo = $"Thanh toan hoa don {hoaDonId}";
            vnp.AddRequestData("vnp_OrderInfo", orderInfo);
            vnp.AddRequestData("vnp_OrderType", orderType);
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);

            // TxnRef = hoaDonId (ngắn gọn cho thuê xe)
            vnp.AddRequestData("vnp_TxnRef", hoaDonId.ToString());

            string paymentUrl = vnp.CreateRequestUrl(baseUrl, hashSecret);
            return Redirect(paymentUrl);
        }

        // RETURN TỪ VNPAY
        [HttpGet("vnpayreturn")]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnp = new VnPayLibrary();

            foreach (var key in Request.Query.Keys)
            {
                if (key.StartsWith("vnp_") &&
                    key != "vnp_SecureHash" &&
                    key != "vnp_SecureHashType")
                {
                    vnp.AddResponseData(key, Request.Query[key]);
                }
            }

            string secureHash = Request.Query["vnp_SecureHash"];
            bool valid = vnp.ValidateSignature(secureHash, _config["VnPay:HashSecret"]);

            if (!valid)
            {
                ViewBag.Message = "Chữ ký VNPAY không hợp lệ.";
                return View("PaymentRentFail");
            }

            // TxnRef = hoaDonId
            string txnRef = vnp.GetResponseData("vnp_TxnRef");
            if (!int.TryParse(txnRef, out int hoaDonId))
            {
                ViewBag.Message = "Mã giao dịch không hợp lệ.";
                return View("PaymentRentFail");
            }

            string responseCode = vnp.GetResponseData("vnp_ResponseCode");

            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTxId == null)
            {
                ViewBag.Message = "Không tìm thấy hóa đơn thuê xe.";
                return View("PaymentRentFail");
            }

            // 00 = thanh toán thành công
            if (responseCode != "00")
            {
                ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                return View("PaymentRentFail");
            }

            await HandleSuccess(hd);
            return View("PaymentRentSuccess");
        }

        // ============================================================
        // 4) PAYPAL
        // ============================================================
        [HttpGet("paypal")]
        public async Task<IActionResult> PayPal(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTxId == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn thuê xe.";
                return RedirectToAction("Index", "ThueXe");
            }

            decimal vndAmount = hd.SoTien ?? 0;
            if (vndAmount <= 0)
            {
                TempData["Error"] = "Số tiền không hợp lệ.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            decimal usd = Math.Round(vndAmount / 24000m, 2);
            if (usd < 0.01m) usd = 0.01m;

            string returnUrl = Url.Action("PayPalReturn", "PaymentRent", new { hoaDonId }, Request.Scheme)!;
            string cancelUrl = Url.Action("PayPalCancel", "PaymentRent", new { hoaDonId }, Request.Scheme)!;

            string? approval = await _payPal.CreateOrderAsync(usd, "USD", returnUrl, cancelUrl);

            if (approval == null)
            {
                TempData["Error"] = "Không tạo được đơn PayPal.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            return Redirect(approval);
        }

        [HttpGet("paypalreturn")]
        public async Task<IActionResult> PayPalReturn(int hoaDonId, string token)
        {
            bool success = await _payPal.CaptureOrderAsync(token);

            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTxId == null)
            {
                ViewBag.Message = "Không tìm thấy hóa đơn thuê xe.";
                return View("PaymentRentFail");
            }

            if (!success)
            {
                ViewBag.Message = "Thanh toán PayPal thất bại hoặc bị hủy.";
                return View("PaymentRentFail");
            }

            await HandleSuccess(hd);
            return View("PaymentRentSuccess");
        }

        [HttpGet("paypalcancel")]
        public IActionResult PayPalCancel()
        {
            ViewBag.Message = "Bạn đã hủy thanh toán PayPal.";
            return View("PaymentRentFail");
        }

        // ============================================================
        // 5) MOMO
        // ============================================================
        [HttpGet("momo")]
        public async Task<IActionResult> MoMo(int hoaDonId)
        {
            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTxId == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn thuê xe.";
                return RedirectToAction("Index", "ThueXe");
            }

            if ((hd.SoTien ?? 0) <= 0)
            {
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("StartPayment", new { hoaDonId });
            }

            string returnUrl = Url.Action("MoMoReturn", "PaymentRent", null, Request.Scheme)!;
            string ipnUrl = Url.Action("MoMoIpn", "PaymentRent", null, Request.Scheme)!;

            string payUrl = await _momo.CreatePaymentUrl(hd, returnUrl, ipnUrl, returnUrl);

            return Redirect(payUrl);
        }

        [HttpGet("momoreturn")]
        public async Task<IActionResult> MoMoReturn()
        {
            var result = await _momo.ProcessReturn(Request.Query);

            if (!result.Success)
            {
                ViewBag.Message = result.Message;
                return View("PaymentRentFail");
            }

            // Momo trả lại orderId = hoaDonId
            string orderId = Request.Query["orderId"];
            if (!int.TryParse(orderId, out int hoaDonId))
            {
                ViewBag.Message = "Mã đơn hàng không hợp lệ.";
                return View("PaymentRentFail");
            }

            var hd = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(h => h.IdThanhToan == hoaDonId);

            if (hd == null || hd.PhieuTxId == null)
            {
                ViewBag.Message = "Không tìm thấy hóa đơn thuê xe.";
                return View("PaymentRentFail");
            }

            await HandleSuccess(hd);
            return View("PaymentRentSuccess");
        }

        [HttpPost("momoipn")]
        public IActionResult MoMoIpn() => Ok();

        // ============================================================
        // 6) XỬ LÝ THÀNH CÔNG CHUNG (VNPAY / PAYPAL / MOMO)
        // ============================================================
        private async Task HandleSuccess(HoaDonThanhToan hd)
        {
            var phieu = await _context.PhieuThueXe
                .Include(p => p.Xe)
                .Include(p => p.User)
                .FirstAsync(p => p.PhieuTxId == hd.PhieuTxId);

            // Cập nhật trạng thái hóa đơn
            hd.TrangThai = true;
            hd.NgayThanhToan = DateTime.Now;
            await _context.SaveChangesAsync();

            // Lấy email + tên
            string? email = phieu.User?.Email
                            ?? HttpContext.Session.GetString("rent_email");
            string? name = phieu.User?.TenDayDu
                            ?? HttpContext.Session.GetString("rent_name")
                            ?? "Khách hàng";

            if (string.IsNullOrWhiteSpace(email))
            {
                // Không có email -> bỏ qua gửi mail
                return;
            }

            var payload = new
            {
                type = "RENT_PAYMENT",
                paymentId = hd.IdThanhToan,
                phieuTxId = hd.PhieuTxId,
                ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonSerializer.Serialize(payload);
            string encrypted = _qrCryptoService.Encrypt(json);
            byte[] qr = GenerateQr(encrypted);

            byte[] pdf = GenerateHoaDonPdf(phieu, hd, name);

            await _mail.SendRentPaymentEmail(
                email, name, phieu.Xe, phieu, hd, qr, pdf);
        }

        // ============================================================
        // 7) QR + PDF
        // ============================================================
        private byte[] GenerateQr(string text)
        {
            using var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            return qr.GetGraphic(8);
        }

        private byte[] GenerateHoaDonPdf(PhieuThueXe phieu, HoaDonThanhToan hd, string customerName)
        {
            string tgBatDau = phieu.TgBatDau?.ToString("dd/MM/yyyy HH:mm") ?? "Không có dữ liệu";
            string soTien = (hd.SoTien ?? 0).ToString("N0");
            string thoiLuong = phieu.TgThue.ToString();
            string fullName = phieu.User?.TenDayDu ?? "Khách hàng";
            var logoPath = Path.Combine(_env.WebRootPath, "images", "Logo", "logo.jpg");
            byte[] logoBytes = System.IO.File.ReadAllBytes(logoPath);

            var model = new
            {
                MaHd = hd.IdThanhToan,
                TenXe = phieu.Xe.LoaiXe,
                BienSo = phieu.Xe.BienSo,
                TgBatDau = tgBatDau,
                ThoiLuong = thoiLuong,
                SoTien = soTien,
                FullName = fullName
            };

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    // ===== HEADER =====
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("DTGPLX CENTER")
                                .Bold().FontSize(20).FontColor("#0066CC");
                            col.Item().Text("Hóa đơn thuê xe")
                                .FontSize(16).Bold().FontColor("#444");
                        });

                        row.ConstantItem(120).Height(60)
                            .AlignRight()
                            .AlignMiddle()
                            .Image(logoBytes)
                            .FitArea();
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Dòng kẻ
                        col.Item().LineHorizontal(1).LineColor("#cccccc");

                        col.Item().PaddingVertical(10).Text("THÔNG TIN KHÁCH HÀNG")
                            .Bold().FontSize(14).FontColor("#444");

                        col.Item().Column(info =>
                        {
                            info.Item().Text($"• Họ tên: {customerName}");
                        });

                        col.Item().PaddingTop(10).Text("CHI TIẾT HÓA ĐƠN")
                            .Bold().FontSize(14).FontColor("#444");

                        // Bảng thông tin
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(180);
                                cols.RelativeColumn();
                            });

                            table.Cell().Text("Mã hóa đơn:").Bold();
                            table.Cell().Text(model.MaHd.ToString());

                            table.Cell().Text("Xe thuê:").Bold();
                            table.Cell().Text($"{model.TenXe} - {model.BienSo}");

                            table.Cell().Text("Thời gian bắt đầu:").Bold();
                            table.Cell().Text(model.TgBatDau);

                            table.Cell().Text("Thời lượng thuê:").Bold();
                            table.Cell().Text($"{model.ThoiLuong} giờ");

                            table.Cell().Text("Số tiền:").Bold();
                            table.Cell().Text($"{model.SoTien} đ").FontColor("#0066CC").Bold();
                        });

                        col.Item().PaddingTop(20).LineHorizontal(1).LineColor("#cccccc");

                        col.Item().PaddingTop(20).Text("Cảm ơn bạn đã sử dụng dịch vụ của DTGPLX Center!")
                            .Italic().FontColor("#777");
                    });

                    // ===== FOOTER =====
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("DTGPLX Center © ").FontColor("#999");
                        txt.Span(DateTime.Now.Year.ToString()).FontColor("#999");
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
