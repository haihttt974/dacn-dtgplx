using dacn_dtgplx.DTOs;
using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class HocController : Controller
    {
        private readonly DtGplxContext _context;

        public HocController(DtGplxContext context)
        {
            _context = context;
        }

        public IActionResult Index(bool open = false)
        {
            var vm = new HocDashboardViewModel();
            // LẤY THÔNG TIN USER & HẠNG ĐÃ CHỌN
            vm.ListHang = _context.Hangs.ToList();
            vm.SelectedHang = HttpContext.Session.GetString("Hang");
            int? userId = HttpContext.Session.GetInt32("UserId");
            // Popup chọn hạng
            vm.ShowPopup = open || string.IsNullOrEmpty(vm.SelectedHang);
            // Nếu chưa chọn hạng → chỉ hiển thị popup
            if (string.IsNullOrEmpty(vm.SelectedHang))
                return View(vm);
            // LẤY THÔNG TIN HẠNG HIỆN TẠI
            var hang = _context.Hangs.FirstOrDefault(h => h.MaHang == vm.SelectedHang);
            if (hang == null)
            {
                vm.ShowPopup = true;
                return View(vm);
            }
            vm.ThoiGianThi = hang.ThoiGianTn / 60;
            vm.SoCauThiNgauNhien = hang.SoCauHoi;
            string hangDaChon = vm.SelectedHang?.Trim().ToUpper();
            bool isXeMay = hangDaChon == "A" || hangDaChon == "A1";
            vm.SelectedHang = hangDaChon;
            // 1️⃣ SỐ BỘ ĐỀ & SỐ ĐỀ ĐÃ LÀM
            vm.TotalBoDe = _context.BoDeThiThus
                .Count(b => b.IdHang == hang.IdHang && b.HoatDong == true);
            vm.DoneBoDe = (userId != null)
                ? _context.BaiLams.Count(b =>
                        b.UserId == userId &&
                        b.IdBoDeNavigation.IdHang == hang.IdHang)
                : 0;
            // 2️⃣ TỔNG SỐ CÂU HỎI LÝ THUYẾT
            if (isXeMay)
            {
                vm.TotalCauHoi = _context.CauHoiLyThuyets
                    .Count(ch => ch.XeMay == true);
            }
            else
            {
                vm.TotalCauHoi = _context.CauHoiLyThuyets.Count();
            }
            // 3️⃣ SỐ CÂU ĐIỂM LIỆT
            vm.TotalCauLiet = _context.CauHoiLyThuyets
                .Where(ch => ch.CauLiet == true && (isXeMay ? ch.XeMay == true : true))
                .Count();
            // 4️⃣ SỐ CÂU DỄ SAI (ChuY)
            vm.TotalCauChuY = _context.CauHoiLyThuyets
                .Where(ch => ch.ChuY == true && (isXeMay ? ch.XeMay == true : true))
                .Count();
            // 5️⃣ SỐ BIỂN BÁO
            vm.TotalBienBao = _context.BienBaos.Count();
            // 6️⃣ PHẦN MÔ PHỎNG – chỉ hiển thị từ B trở lên
            vm.HasMoPhong = !isXeMay;  // B, C, D, E, F
            if (vm.HasMoPhong)
            {
                vm.MpBoDe = _context.BoDeMoPhongs
                    .Count(mp => mp.IsActive == true);
                vm.MpTinhHuong = _context.TinhHuongMoPhongs.Count();

                vm.MpBoDeDone = (userId != null)
                    ? _context.BaiLamMoPhongs
                        .Count(b => b.UserId == userId && b.IdBoDeMoPhongNavigation.IsActive == true)
                    : 0;
            }
            return View(vm);
        }
        public IActionResult CauLiet()
        {
            // 1. Lấy hạng đang chọn
            string hang = HttpContext.Session.GetString("Hang")?.Trim().ToUpper();
            bool isXeMay = hang == "A" || hang == "A1";

            // 2. Lấy toàn bộ câu hỏi giống HocAll
            var allQuestions = _context.CauHoiLyThuyets
                .Include(c => c.Chuong)
                .Include(c => c.DapAns)
                .OrderBy(c => c.Chuong.ThuTu)
                .ThenBy(c => c.IdCauHoi)
                .ToList();

            // 3. Gán GlobalIndex chung
            int globalIndex = 1;
            var mappedAll = allQuestions.Select(q => new
            {
                Question = q,
                GlobalIndex = globalIndex++
            }).ToList();

            // 4. Lọc câu điểm liệt
            var cauLiet = mappedAll
                .Where(x =>
                    x.Question.CauLiet == true &&
                    (isXeMay ? x.Question.XeMay == true : true)
                )
                .ToList();

            // 5. Gom lại theo chương + chuẩn hóa dữ liệu
            var chapters = cauLiet
                .GroupBy(x => x.Question.Chuong)
                .Select(g => new HocAllChapterVM
                {
                    ChuongId = g.Key.ChuongId,
                    TenChuong = g.Key.TenChuong,
                    ThuTu = g.Key.ThuTu ?? 0,

                    Questions = g.Select(x => new HocAllQuestionVM
                    {
                        GlobalIndex = x.GlobalIndex,
                        IdCauHoi = x.Question.IdCauHoi,
                        NoiDung = x.Question.NoiDung ?? "",

                        ImageUrl = NormalizeImage(x.Question.HinhAnh),
                        UrlAnhMeo = NormalizeImage(x.Question.UrlAnhMeo),

                        IsCauLiet = true,
                        IsChuY = x.Question.ChuY ?? false,
                        IsXeMay = x.Question.XeMay ?? false,

                        DapAns = x.Question.DapAns
                        .OrderBy(d => d.ThuTu)
                        .Select((d, idx) => new HocAllAnswerVM
                        {
                            IdDapAn = d.IdDapAn,
                            Label = (idx + 1).ToString(),   // ⭐ số thứ tự: 1,2,3
                            IsCorrect = d.DapAnDung == true
                        }).ToList()

                    }).ToList()
                })
                .OrderBy(c => c.ThuTu)
                .ToList();

            // 6. Build ViewModel
            var vm = new HocAllViewModel
            {
                SelectedHang = hang,
                IsXeMay = isXeMay,
                Chapters = chapters,
                TotalQuestions = chapters.Sum(c => c.Questions.Count),
                TotalChapters = chapters.Count
            };

            return View(vm);
        }
        public IActionResult ChuY()
        {
            // 1. Lấy hạng đang chọn
            string hang = HttpContext.Session.GetString("Hang")?.Trim().ToUpper();
            bool isXeMay = hang == "A" || hang == "A1";

            // 2. Lấy toàn bộ câu hỏi giống HocAll
            var allQuestions = _context.CauHoiLyThuyets
                .Include(c => c.Chuong)
                .Include(c => c.DapAns)
                .OrderBy(c => c.Chuong.ThuTu)
                .ThenBy(c => c.IdCauHoi)
                .ToList();

            // 3. Gán GlobalIndex chung
            int globalIndex = 1;
            var mappedAll = allQuestions.Select(q => new
            {
                Question = q,
                GlobalIndex = globalIndex++
            }).ToList();

            // 4. Lọc câu CHÚ Ý (ChuY = 1)
            var cauChuY = mappedAll
                .Where(x =>
                    (x.Question.ChuY ?? false) == true &&
                    (isXeMay ? (x.Question.XeMay ?? false) == true : true)
                )
                .ToList();

            // 5. Gom lại theo chương + chuẩn hóa dữ liệu
            var chapters = cauChuY
                .GroupBy(x => x.Question.Chuong)
                .Select(g => new HocAllChapterVM
                {
                    ChuongId = g.Key.ChuongId,
                    TenChuong = g.Key.TenChuong,
                    ThuTu = g.Key.ThuTu ?? 0,

                    Questions = g.Select(x => new HocAllQuestionVM
                    {
                        GlobalIndex = x.GlobalIndex,
                        IdCauHoi = x.Question.IdCauHoi,
                        NoiDung = x.Question.NoiDung ?? "",

                        ImageUrl = NormalizeImage(x.Question.HinhAnh),
                        UrlAnhMeo = NormalizeImage(x.Question.UrlAnhMeo),

                        IsCauLiet = x.Question.CauLiet ?? false,
                        IsChuY = true,                       // ⭐ đang lọc theo ChuY
                        IsXeMay = x.Question.XeMay ?? false,

                        DapAns = x.Question.DapAns
                            .OrderBy(d => d.ThuTu)
                            .Select((d, idx) => new HocAllAnswerVM
                            {
                                IdDapAn = d.IdDapAn,
                                Label = (idx + 1).ToString(),
                                IsCorrect = d.DapAnDung == true
                            }).ToList()

                    }).ToList()
                })
                .OrderBy(c => c.ThuTu)
                .ToList();

            // 6. Build ViewModel
            var vm = new HocAllViewModel
            {
                SelectedHang = hang,
                IsXeMay = isXeMay,
                Chapters = chapters,
                TotalQuestions = chapters.Sum(c => c.Questions.Count),
                TotalChapters = chapters.Count
            };

            return View(vm);
        }

        //  CHUẨN HÓA ĐƯỜNG DẪN ẢNH
        private string? NormalizeImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            fileName = fileName.Trim();

            // Fix lỗi DB: wwwwroot/...
            if (fileName.StartsWith("wwwwroot"))
                fileName = fileName.Replace("wwwwroot", "").TrimStart('/');

            if (fileName.StartsWith("wwwroot"))
                fileName = fileName.Replace("wwwroot", "").TrimStart('/');

            // ~/images/... → /images/...
            if (fileName.StartsWith("~/"))
                return fileName.Replace("~/", "/");

            // images/... → /images/...
            if (fileName.StartsWith("images"))
                return "/" + fileName;

            if (fileName.StartsWith("/images"))
                return fileName;

            // Nếu chỉ có tên file → coi như ảnh câu hỏi
            return "/images/cau_hoi/" + fileName;
        }
        [HttpPost]
        public IActionResult ChonHang(string maHang)
        {
            if (!string.IsNullOrEmpty(maHang))
                HttpContext.Session.SetString("Hang", maHang);

            return RedirectToAction("Index");
        }

        public IActionResult FlashCardBienBao()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            bool isLoggedIn = userId.HasValue && userId.Value > 0;

            // 1️⃣ Query SQL THUẦN (KHÔNG gọi method)
            var cards = _context.BienBaos
                .Select(b => new BienBaoFlashCardVM
                {
                    IdBienBao = b.IdBienBao,
                    TenBienBao = b.TenBienBao,
                    YNghia = b.Ynghia,
                    HinhAnh = b.HinhAnh, // CHỈ LẤY RAW

                    IdFlashcard = isLoggedIn
                        ? b.FlashCards
                            .Where(f => f.UserId == userId.Value)
                            .Select(f => (int?)f.IdFlashcard)
                            .FirstOrDefault()
                        : null,

                    DanhGia = isLoggedIn
                        ? b.FlashCards
                            .Where(f => f.UserId == userId.Value)
                            .Select(f => f.DanhGia)
                            .FirstOrDefault()
                        : null
                })
                .ToList(); // 🔥 RA KHỎI EF

            // 2️⃣ XỬ LÝ C# THUẦN
            foreach (var c in cards)
            {
                c.HinhAnh = NormalizeImage(c.HinhAnh);
            }

            var vm = new BienBaoFlashStudyPageVM
            {
                IsLoggedIn = isLoggedIn,
                LoginUrl = "/Auth/Login",
                Cards = cards
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Save([FromBody] SaveFlashcardDto dto)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value <= 0)
                return Unauthorized(new { message = "Bạn cần đăng nhập để lưu tiến trình." });

            var fc = _context.FlashCards
                .FirstOrDefault(x => x.UserId == userId.Value && x.IdBienBao == dto.IdBienBao);

            if (fc == null)
            {
                fc = new FlashCard
                {
                    UserId = userId.Value,
                    IdBienBao = dto.IdBienBao,
                    DanhGia = dto.DanhGia
                };
                _context.FlashCards.Add(fc);
            }
            else
            {
                fc.DanhGia = dto.DanhGia;
            }

            _context.SaveChanges();

            return Ok(new
            {
                ok = true,
                idBienBao = dto.IdBienBao,
                danhGia = dto.DanhGia
            });
        }
    }
}
