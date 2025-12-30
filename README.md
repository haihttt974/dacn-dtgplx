# 🚗 Driving License Training and Examination System (Đề tài tốt nghiệp – ĐTGPLX)

[![PHP 8.1+](https://img.shields.io/badge/PHP-8.1%2B-777BB4?logo=php&logoColor=white)](https://www.php.net/)
[![MySQL](https://img.shields.io/badge/Database-MySQL-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

Ứng dụng web hỗ trợ **luyện thi và thi thử lý thuyết bằng lái xe máy A1, A2 và ô tô B1, B2** theo bộ **600 câu hỏi chính thức** của Bộ GTVT. Hệ thống phục vụ cả người học và quản trị viên, cung cấp thống kê, lịch sử thi, cùng giao diện thân thiện trên mọi thiết bị – phù hợp trình diễn cho đồ án tốt nghiệp.

---

## 📑 Mục lục
- [Tổng quan](#-tổng-quan)
- [Tính năng chính](#-tính-năng-chính)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Kiến trúc & mô-đun](#-kiến-trúc--mô-đun)
- [Cài đặt & triển khai](#-cài-đặt--triển-khai)
- [Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng)
- [Ảnh minh họa](#-ảnh-minh-họa)
- [Cấu trúc thư mục tham khảo](#-cấu-trúc-thư-mục-tham-khảo)
- [Câu hỏi thường gặp](#-câu-hỏi-thường-gặp)
- [Đóng góp](#-đóng-góp)
- [Giấy phép](#-giấy-phép)
- [Tác giả & liên hệ](#-tác-giả--liên-hệ)

---

## 🧭 Tổng quan
- **Mục tiêu:** Tạo môi trường luyện tập bám sát bài thi thật, chấm điểm tự động, hiển thị lỗi nghiêm trọng để người học biết và khắc phục.
- **Đối tượng:** Người học lái xe (A1, A2, B1, B2) và quản trị viên trung tâm đào tạo.
- **Giá trị nổi bật:**
  - Dữ liệu câu hỏi chuẩn hóa, cập nhật theo bộ đề 600 câu.
  - Trải nghiệm thi thử mô phỏng thực tế: giới hạn thời gian, quy tắc câu điểm liệt.
  - Bảng điều khiển quản trị tập trung cho tài khoản, câu hỏi, bộ đề và kết quả.

## ✨ Tính năng chính
- 🔐 **Đăng ký & đăng nhập** cho học viên và quản trị viên.
- 🧠 **Luyện tập theo chủ đề** hoặc toàn bộ 600 câu, có gợi ý đáp án và giải thích (nếu cung cấp).
- ⏱️ **Thi thử mô phỏng thật**: đếm thời gian, chấm tự động, cảnh báo câu điểm liệt.
- 📊 **Thống kê & lịch sử thi**: xem điểm, tỷ lệ đúng, danh sách câu sai, xu hướng tiến bộ.
- 🗂️ **Quản trị nội dung**: quản lý người dùng, câu hỏi, bộ câu hỏi, kết quả và cài đặt.
- 📱 **Giao diện đáp ứng**: hỗ trợ desktop, tablet, mobile với Bootstrap.

## 🛠 Công nghệ sử dụng
- **Frontend:** HTML, CSS, JavaScript, Bootstrap.
- **Backend:** PHP.
- **Cơ sở dữ liệu:** MySQL.
- **Thư viện khác:** jQuery (nếu sử dụng), cùng các tiện ích front-end/back-end liên quan.

## 🧩 Kiến trúc & mô-đun
| Thành phần | Mô tả ngắn |
| --- | --- |
| Giao diện người học | Trang chủ, luyện tập theo chủ đề, thi thử, xem lịch sử. |
| Giao diện quản trị | Bảng điều khiển, quản lý người dùng, câu hỏi, bộ đề, kết quả. |
| Lớp dịch vụ | Xử lý logic thi, chấm điểm, quy tắc câu điểm liệt, lưu kết quả. |
| Tầng dữ liệu | Kết nối MySQL, truy vấn câu hỏi, đề thi, thống kê. |

## 🚀 Cài đặt & triển khai
> Yêu cầu: PHP 8.1+ (hoặc bản tương thích), MySQL/MariaDB, môi trường web server (XAMPP/WAMP/LAMP).

1. **Clone mã nguồn**
   ```bash
   git clone https://github.com/haihtt974/dacn-dtgplx.git
   cd dacn-dtgplx
   ```
2. **Khởi tạo cơ sở dữ liệu**
   - Tạo database MySQL (ví dụ: `dtgplx`).
   - Import file SQL đi kèm (ví dụ: `database/dtgplx.sql` hoặc file tương ứng).
3. **Cấu hình kết nối**
   - Mở file cấu hình (ví dụ: `config.php` hoặc `db_connection.php`).
   - Cập nhật thông tin host, database, username, password.
4. **Triển khai lên web server**
   - Đặt mã nguồn vào thư mục public của server (vd: `htdocs/dacn-dtgplx` trong XAMPP).
   - Bật Apache + MySQL, đảm bảo PHP extension `mysqli`/`pdo_mysql` hoạt động.
5. **Truy cập ứng dụng**
   - Mở trình duyệt: `http://localhost/dacn-dtgplx`.

## 🎯 Hướng dẫn sử dụng
- **Học viên:**
  1) Truy cập trang chủ (`index.php`) và đăng ký tài khoản.
  2) Chọn **Luyện tập** theo chủ đề hoặc toàn bộ câu hỏi.
  3) Chọn **Thi thử** để vào bài thi mô phỏng (đếm thời gian, tính điểm).
  4) Xem **Lịch sử** để rà soát điểm, câu sai, câu điểm liệt.
- **Quản trị viên:**
  1) Đăng nhập bằng tài khoản quản trị (dùng mật khẩu mặc định nếu được cung cấp, hoặc cập nhật trong CSDL).
  2) Quản lý **người dùng**, **câu hỏi**, **bộ đề**, **kết quả thi**.
  3) Theo dõi thống kê điểm, tỉ lệ đỗ, mức độ hoàn thành của học viên.

## 🖼️ Ảnh minh họa
*(Thay thế bằng ảnh thực tế của dự án)*
- Trang chủ: `docs/screenshots/homepage.png`
- Màn hình thi thử: `docs/screenshots/mock-exam.png`
- Bảng điều khiển admin: `docs/screenshots/admin-dashboard.png`

## 🗂 Cấu trúc thư mục tham khảo
```
dacn-dtgplx/
├─ public/               # Tệp PHP/HTML chính, router, assets tĩnh
├─ src/                  # Business logic, services, repository
├─ config.php            # Cấu hình kết nối CSDL
├─ database/             # File .sql khởi tạo (nếu có)
├─ docs/screenshots/     # Ảnh minh họa README
└─ README.md             # Tài liệu này
```

## ❓ Câu hỏi thường gặp
- **Có hỗ trợ câu điểm liệt?** Có, hệ thống gắn cờ các câu điểm liệt và áp dụng quy tắc rớt nếu trả lời sai.  
- **Thi thử có giới hạn thời gian không?** Có, đồng hồ đếm ngược mô phỏng bài thi thật.  
- **Có thể xuất thống kê không?** Quản trị có thể xem thống kê điểm, tỷ lệ đỗ và lịch sử từng học viên; có thể mở rộng để xuất CSV/Excel.  

## 🤝 Đóng góp
Đóng góp, báo lỗi hoặc đề xuất tính năng mới luôn được hoan nghênh!
1. Fork repo & tạo nhánh tính năng.
2. Commit thay đổi có mô tả rõ ràng.
3. Mở Pull Request nêu mục tiêu, mô tả chi tiết và minh họa (nếu có).

## 📄 Giấy phép
Dự án sử dụng giấy phép **MIT** (hoặc giấy phép đi kèm nếu được cập nhật trong repo). Vui lòng kiểm tra tệp `LICENSE` khi bổ sung.

## 👤 Tác giả & liên hệ
- **Tác giả:** haihtt974  
- Email liên hệ (gợi ý): `haihtt974@example.com`  
- Nếu dùng trong đồ án, vui lòng ghi rõ nguồn và trích dẫn tác giả.

---

✨ *Gợi ý:* Hãy thêm ảnh chụp màn hình thực tế, cập nhật link demo (nếu có) và mô tả ngắn gọn quá trình bạn thực hiện/đóng góp trong đồ án để README nổi bật hơn!
