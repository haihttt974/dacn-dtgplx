using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace dacn_dtgplx.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IWebHostEnvironment _env;

        public InvoiceService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GenerateInvoicePdf(HoaDonThanhToan bill)
        {
            // ---------------- LOGO ----------------
            // wwwroot/images/Logo/logo.png
            string logoPath = Path.Combine(_env.WebRootPath, "images", "Logo", "logo.jpg");
            byte[]? logoBytes = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;

            // ---------------- THÔNG TIN CHUNG ----------------
            string maHd = bill.IdThanhToan.ToString();
            string ngayThanhToan = bill.NgayThanhToan?.ToString("dd/MM/yyyy HH:mm")
                                   ?? "Chưa thanh toán";
            string soTien = (bill.SoTien ?? 0).ToString("N0") + " đ";
            string phuongThuc = bill.PhuongThucThanhToan ?? "Không rõ";
            string trangThai =
                bill.TrangThai == true ? "Thành công" :
                bill.TrangThai == false ? "Thất bại" : "Chưa thanh toán";

            bool isKhoaHoc = bill.IdDangKyNavigation != null;
            bool isThueXe = bill.PhieuTx != null;

            // --------- KHÁCH HÀNG ---------
            string khachHang = "Khách hàng";

            if (isKhoaHoc && bill.IdDangKyNavigation?.HoSo?.User != null)
            {
                khachHang = bill.IdDangKyNavigation.HoSo.User.TenDayDu
                            ?? bill.IdDangKyNavigation.HoSo.User.Username;
            }
            else if (isThueXe && bill.PhieuTx?.User != null)
            {
                khachHang = bill.PhieuTx.User.TenDayDu
                            ?? bill.PhieuTx.User.Username;
            }

            // --------- CHI TIẾT ---------
            string tieuDeChiTiet;
            string dong1 = "", dong2 = "", dong3 = "";

            if (isKhoaHoc)
            {
                tieuDeChiTiet = "Chi tiết khóa học";
                dong1 = "Khóa học: " + (bill.IdDangKyNavigation!.KhoaHoc?.TenKhoaHoc ?? "(Không xác định)");
                dong2 = "Mã đăng ký: " + bill.IdDangKyNavigation.IdDangKy;
                dong3 = "Ghi chú: " + (bill.NoiDung ?? "Không có");
            }
            else if (isThueXe)
            {
                var p = bill.PhieuTx!;
                tieuDeChiTiet = "Chi tiết thuê xe";
                dong1 = $"Xe: {p.Xe?.LoaiXe} - {p.Xe?.BienSo}";
                dong2 = $"Thời gian bắt đầu: {p.TgBatDau?.ToString("dd/MM/yyyy HH:mm") ?? "Không rõ"}";
                dong3 = $"Thời lượng: {p.TgThue} giờ";
            }
            else
            {
                tieuDeChiTiet = "Chi tiết hóa đơn";
                dong1 = "Ghi chú: " + (bill.NoiDung ?? "Không có");
            }

            // ---------------- TẠO PDF ----------------
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor("#222"));

                    // ===== HEADER =====
                    page.Header().Row(row =>
                    {
                        // Bên trái: tên trung tâm + tiêu đề
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("DTGPLX CENTER")
                                .SemiBold().FontSize(22).FontColor("#0d6efd");

                            col.Item().Text("Hóa đơn thanh toán")
                                .FontSize(13).FontColor("#555");
                        });

                        // Bên phải: LOGO trong ô vuông (như hình bạn khoanh đỏ)
                        if (logoBytes != null)
                        {
                            row.ConstantItem(80).AlignRight().Element(c =>
                            {
                                c.Height(80)       // chiều cao ô logo
                                 .AlignRight()
                                 .AlignMiddle()
                                 .Image(logoBytes, ImageScaling.FitArea); // tự co giãn vừa ô
                            });
                        }
                    });

                    // ===== NỘI DUNG =====
                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // Thông tin hóa đơn
                        col.Item().BorderBottom(1).BorderColor("#e1e1e1").PaddingBottom(10)
                           .Row(row =>
                           {
                               row.RelativeItem().Column(c =>
                               {
                                   c.Item().Text("Thông tin hóa đơn").SemiBold().FontSize(13);
                                   c.Item().Text(t => { t.Span("Mã hóa đơn: ").SemiBold(); t.Span(maHd); });
                                   c.Item().Text(t => { t.Span("Ngày thanh toán: ").SemiBold(); t.Span(ngayThanhToan); });
                               });

                               row.RelativeItem().Column(c =>
                               {
                                   c.Item().Text("Thanh toán").SemiBold().FontSize(13);
                                   c.Item().Text(t =>
                                   {
                                       t.Span("Số tiền: ").SemiBold();
                                       t.Span(soTien).FontColor("#0d6efd");
                                   });
                                   c.Item().Text(t =>
                                   {
                                       t.Span("Phương thức: ").SemiBold();
                                       t.Span(phuongThuc);
                                   });
                               });
                           });

                        // Thông tin khách hàng
                        col.Item().PaddingTop(15).Column(c =>
                        {
                            c.Item().Text("Thông tin khách hàng").SemiBold().FontSize(13);
                            c.Item().Text(t =>
                            {
                                t.Span("Họ tên: ").SemiBold();
                                t.Span(khachHang);
                            });
                            c.Item().Text(t =>
                            {
                                t.Span("Trạng thái: ").SemiBold();
                                t.Span(trangThai);
                            });
                        });

                        // Chi tiết
                        col.Item().PaddingTop(20).Column(c =>
                        {
                            c.Item().Text(tieuDeChiTiet).SemiBold().FontSize(13);
                            c.Item().Text(dong1);
                            if (!string.IsNullOrWhiteSpace(dong2)) c.Item().Text(dong2);
                            if (!string.IsNullOrWhiteSpace(dong3)) c.Item().Text(dong3);
                        });

                        // Tổng kết
                        col.Item().PaddingTop(25).AlignRight().Column(c =>
                        {
                            c.Item().Text("TỔNG THANH TOÁN").SemiBold().FontSize(12);
                            c.Item().Text(soTien).FontSize(16).SemiBold().FontColor("#0d6efd");
                        });
                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Cảm ơn bạn đã sử dụng dịch vụ của DTGPLX Center.")
                         .FontSize(10).FontColor("#888");
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
