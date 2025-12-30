# 🚗 Hệ thống Luyện thi & Quản lý GPLX (dacn-dtgplx)

Ứng dụng ASP.NET Core MVC hỗ trợ luyện thi lý thuyết, mô phỏng tình huống giao thông, quản lý khóa học, thuê xe tập lái và thanh toán trực tuyến cho trung tâm đào tạo lái xe. Hệ thống cung cấp trang người học, giáo viên, quản trị, cùng chatbot hỗ trợ và các tiện ích báo cáo.

---

## 📑 Mục lục
- [Tổng quan tính năng](#-tổng-quan-tính-năng)
- [Công nghệ & thư viện](#-công-nghệ--thư-viện)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Cài đặt & cấu hình](#-cài-đặt--cấu-hình)
- [Luồng sử dụng chính](#-luồng-sử-dụng-chính)
- [Cấu hình thanh toán & tích hợp](#-cấu-hình-thanh-toán--tích-hợp)
- [Công cụ Python cho OCR & tìm kiếm](#-công-cụ-python-cho-ocr--tìm-kiếm)
- [Ảnh minh họa](#-ảnh-minh-họa)
- [Đóng góp](#-đóng-góp)
- [Giấy phép](#-giấy-phép)

---

## 🧭 Tổng quan tính năng
### Người học
- Đăng ký/đăng nhập, lưu session, xác thực JWT nội bộ (dùng cho API) và đăng nhập mạng xã hội (Google, Facebook).【F:Controllers/AuthController.cs†L1-L214】
- Chọn hạng GPLX, xem tổng quan câu hỏi, câu liệt, câu cần chú ý, tổng số biển báo, tình huống mô phỏng.【F:Controllers/HocController.cs†L1-L94】
- Luyện tập lý thuyết theo chương, câu liệt, câu chú ý; xem đáp án và hình ảnh; học bằng flashcard biển báo.【F:Controllers/HocController.cs†L95-L215】【F:Controllers/AdminFlashCardsController.cs†L1-L160】
- Thi thử trắc nghiệm theo bộ đề, chấm điểm tự động, lưu kết quả, xem lại lịch sử bài làm.【F:Controllers/LyThuyetController.cs†L1-L210】
- Thi mô phỏng 10 tình huống: tính điểm theo thời điểm nhấn, chấm tự động, lưu kết quả và thống kê sai đúng từng tình huống.【F:Controllers/ThiMoPhongController.cs†L1-L200】
- Xem khóa học, đăng ký, lịch học, lịch dạy; theo dõi hóa đơn và tiến độ học tập.【F:Controllers/KhoaHocController.cs†L1-L200】【F:Controllers/ThoiKhoaBieuController.cs†L1-L200】
- Thuê xe tập lái: lọc theo loại xe/giá, kiểm tra trùng lịch học/phiếu thuê đã thanh toán, đặt thuê và thanh toán.【F:Controllers/ThueXeController.cs†L1-L200】【F:Controllers/PaymentRentController.cs†L1-L200】
- Chatbot và chat hai chiều (SignalR) hỗ trợ trao đổi; theo dõi người dùng online.【F:Hubs/ChatHub.cs†L1-L200】【F:Controllers/ChatbotController.cs†L1-L200】
- Nhận thông báo, phản hồi và xem thông báo hệ thống.【F:Controllers/NotificationController.cs†L1-L200】

### Quản trị viên
- Bảng điều khiển tổng quan.【F:Controllers/AdminDashboardController.cs†L1-L200】
- Quản lý người dùng, hồ sơ học viên, giáo viên, khóa học, lớp học, lịch dạy, lịch học.【F:Controllers/AdminUsersController.cs†L1-L220】【F:Controllers/AdminProfilesController.cs†L1-L220】
- Quản lý câu hỏi trắc nghiệm, bộ đề, bài làm; quản lý tình huống mô phỏng, bộ đề mô phỏng, kết quả mô phỏng.【F:Controllers/AdminTheoryQuestionsController.cs†L1-L220】【F:Controllers/AdminSimulationExamSetsController.cs†L1-L200】
- Quản lý biển báo, flashcard, biển báo cho học viên.【F:Controllers/AdminSignsController.cs†L1-L200】【F:Controllers/AdminFlashCardsController.cs†L1-L160】
- Quản lý xe tập lái, quét mã QR phiếu thuê, hóa đơn thanh toán (khoá học và thuê xe).【F:Controllers/AdminVehiclesController.cs†L1-L220】【F:Controllers/AdminPaymentsController.cs†L1-L200】
- Báo cáo, xuất thống kê, phản hồi người dùng, gửi thông báo hàng loạt.【F:Controllers/AdminReportController.cs†L1-L200】【F:Controllers/AdminFeedbacksController.cs†L1-L200】【F:Controllers/AdminNotificationsController.cs†L1-L200】

---

## 🛠 Công nghệ & thư viện
- **Nền tảng:** ASP.NET Core 9.0 MVC, Razor Views, Session + Cookie Auth.
- **CSDL:** Entity Framework Core với SQL Server (`DefaultConnection`).【F:Program.cs†L13-L26】
- **Realtime:** SignalR (ChatHub, OnlineHub).【F:Program.cs†L72-L92】
- **Thanh toán:** VNPAY (CinemaS.VNPAY), PayPal SDK, MoMo API, mã QR hóa đơn/phiếu thuê xe.【F:Controllers/PaymentController.cs†L1-L200】【F:Services/VnPayLibrary.cs†L1-L200】
- **Bảo mật & mã hóa:** BCrypt.Net, JWT Bearer, CryptoSettings, Steganography (ẩn dữ liệu ảnh).【F:Program.cs†L27-L71】
- **Tích hợp ngoài:** Google/Facebook OAuth, MailKit/MimeKit gửi email, Swagger cho API.【F:Program.cs†L36-L70】
- **Tài liệu & PDF:** QuestPDF, QRCoder, SkiaSharp/ImageSharp cho xử lý ảnh.【F:dacn-dtgplx.csproj†L1-L35】
- **AI/Chatbot & tìm kiếm:** OpenAI SDK, embedding câu hỏi (file `PythonScripts/questions_with_emb.json`), ChatbotController, AiChatService.【F:Services/AiChatService.cs†L1-L200】
- **Frontend:** Bootstrap, jQuery (trong Views/wwwroot), Razor partials.

---

## 🗂 Cấu trúc dự án
```
dacn-dtgplx/
├─ Controllers/              # MVC controllers (auth, học/thi, thanh toán, admin,...)
├─ Models/                   # Entity Framework Core models & DbContext
├─ ViewModels/DTOs/Helpers/  # View models, DTO, tiện ích controller/session/image
├─ Services/                 # Mail, thanh toán (VNPAY/PayPal/MoMo), QR, báo cáo, AI chat, steganography
├─ Hubs/                     # SignalR hubs (chat, online presence)
├─ Views/                    # Razor views
├─ wwwroot/                  # Static assets
├─ PythonScripts/            # OCR + embedding câu hỏi, tìm kiếm semantic
├─ appsettings.Development.json  # Mẫu cấu hình logging (không chứa chuỗi kết nối)
├─ dacn-dtgplx.csproj        # Tham chiếu package .NET 9.0
└─ README.md
```

---

## ⚙️ Cài đặt & cấu hình
> Yêu cầu: .NET SDK 9.0+, SQL Server (hoặc SQL Server Express/Azure SQL), Node.js tùy nhu cầu build front-end, Git.

1) **Clone mã nguồn**
```bash
git clone https://github.com/haihtt974/dacn-dtgplx.git
cd dacn-dtgplx
```

2) **Khai báo chuỗi kết nối**
- Tạo `appsettings.json` (hoặc bổ sung vào `appsettings.Development.json`) với `ConnectionStrings:DefaultConnection` trỏ tới SQL Server của bạn.

3) **Chuẩn bị cơ sở dữ liệu**
- Dự án dùng DbContext `DtGplxContext` ánh xạ nhiều bảng (bài làm, câu hỏi, biển báo, khóa học, thuê xe, hóa đơn...). Repo hiện **không kèm file migration hay script .sql**, bạn cần phục hồi CSDL tương ứng (ví dụ từ bản backup hiện có) hoặc tự scaffold lại schema phù hợp với các model trong `Models/`.

4) **Cấu hình tích hợp (tùy chọn nhưng cần cho tính năng tương ứng)**
- `Authentication:Google`, `Authentication:Facebook`: ClientId/Secret.
- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`: phục vụ JWT Bearer.
- `VnPay:*`, `Momo:*`, `PayPal:*`: khóa/secret/URL trả về cho thanh toán.
- `CryptoSettings`, `Steganography`: cấu hình mã hóa/giấu tin nếu dùng.
- Mail (MailKit) cho gửi email xác nhận/hóa đơn.

5) **Restore & chạy**
```bash
dotnet restore
dotnet run --project dacn-dtgplx/dacn-dtgplx.csproj
```
Ứng dụng mặc định lắng nghe `http://localhost:5000` và `https://localhost:5001` (theo cấu hình Kestrel).

---

## 🚦 Luồng sử dụng chính
- **Xác thực:** Đăng ký, đăng nhập (username/email + mật khẩu băm BCrypt), lưu JWT vào session; hỗ trợ đăng nhập Google/Facebook.【F:Controllers/AuthController.cs†L1-L214】
- **Chọn hạng & học:** Chọn hạng GPLX, xem thống kê câu hỏi/câu liệt/biển báo/tình huống; học lý thuyết theo chương, câu liệt, câu chú ý; xem biển báo và flashcard.【F:Controllers/HocController.cs†L1-L215】
- **Thi thử trắc nghiệm:** Vào bộ đề, làm bài, chấm điểm, lưu kết quả, xem lại lịch sử và câu sai.【F:Controllers/LyThuyetController.cs†L1-L210】
- **Thi mô phỏng:** Làm 10 tình huống, hệ thống tính điểm theo thời điểm nhấn, lưu tổng điểm và điểm từng tình huống.【F:Controllers/ThiMoPhongController.cs†L1-L200】
- **Khóa học & lịch học:** Xem, đăng ký khóa học; theo dõi lịch học/lịch dạy; tự động cập nhật trạng thái khóa học (AutoUpdateKhoaHocService).【F:Controllers/KhoaHocController.cs†L1-L200】
- **Thuê xe & thanh toán:** Lọc xe, kiểm tra trùng lịch, đặt thuê; thanh toán qua VNPAY/PayPal/MoMo; sinh QR cho phiếu thuê/hóa đơn.【F:Controllers/ThueXeController.cs†L1-L200】【F:Controllers/PaymentRentController.cs†L1-L200】
- **Chat & chatbot:** Chat realtime, chatbot hỗ trợ nội dung GPLX, theo dõi người dùng online (SignalR).【F:Hubs/ChatHub.cs†L1-L200】【F:Controllers/ChatbotController.cs†L1-L200】
- **Thông báo & phản hồi:** Gửi/nhận thông báo, đánh dấu đã xem, quản lý phản hồi và thống kê trong trang quản trị.【F:Controllers/AdminNotificationsController.cs†L1-L200】
- **Trang quản trị:** Quản lý người dùng, hồ sơ, câu hỏi/bộ đề, mô phỏng, biển báo, flashcard, xe tập lái, hóa đơn, báo cáo, phản hồi, thông báo.【F:Services/FeatureService.cs†L16-L64】

---

## 💳 Cấu hình thanh toán & tích hợp
- **VNPAY:** `VnPay:BaseUrl`, `VnPay:TmnCode`, `VnPay:HashSecret`, `VnPay:OrderType`, `VnPay:Locale`, `VnPay:CurrCode`. Trả về tại `/payment/vnpayreturn`.【F:Controllers/PaymentController.cs†L40-L132】
- **MoMo:** Khóa/endpoint từ cấu hình `Momo:*`, dùng trong `MomoService`.【F:Services/MomoService.cs†L1-L200】
- **PayPal:** ClientId/Secret, môi trường (sandbox/live) từ `PayPal:*`, dùng trong `PayPalService`.【F:Services/PayPalService.cs†L1-L200】
- **QR & hóa đơn:** `QrService`, `QrCryptoService`, `InvoiceService` tạo mã QR/ảnh hóa đơn, gửi email qua `MailService`.【F:Services/QrService.cs†L1-L200】
- **OAuth:** `Authentication:Google` và `Authentication:Facebook` cho đăng nhập mạng xã hội.【F:Program.cs†L36-L64】

---

## 🤖 Công cụ Python cho OCR & tìm kiếm
- Thư mục `PythonScripts/` chứa pipeline OCR và tạo embedding câu hỏi (pytesseract, sentence-transformers, openai).【F:PythonScripts/README.md†L1-L120】
- File `questions_with_emb.json` được nạp vào dịch vụ tìm kiếm/AI tại runtime.【F:Program.cs†L98-L115】
- Thiết lập Python 3.9+, cài thư viện từ `PythonScripts/requirement.txt`, cài Tesseract OCR (hướng dẫn trong `PythonScripts/README.md`).

---

## 🖼️ Ảnh minh họa
- Trang chủ: `docs/screenshots/home.png`
- Luyện tập/thi thử: `docs/screenshots/exam.png`
- Thi mô phỏng: `docs/screenshots/simulation.png`
- Trang quản trị: `docs/screenshots/admin-dashboard.png`

*(Thêm ảnh thực tế vào thư mục `docs/screenshots/` để hiển thị.)*

---

## 🤝 Đóng góp
1. Tạo nhánh mới từ `main`.
2. Thực hiện thay đổi, mô tả rõ ràng khi commit.
3. Mở Pull Request kèm mô tả tính năng/sửa lỗi và ảnh minh họa (nếu có).

---

## 📄 Giấy phép
Kho lưu trữ hiện **chưa cung cấp tệp LICENSE**. Vui lòng bổ sung giấy phép trước khi phân phối hoặc sử dụng công khai.
