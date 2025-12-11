using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class CauLietController : Controller
    {
        private readonly DtGplxContext _context;

        public CauLietController(DtGplxContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ==============================
            // 1. Lấy hạng đang chọn
            // ==============================
            string hang = HttpContext.Session.GetString("Hang")?.Trim().ToUpper();
            bool isXeMay = hang == "A" || hang == "A1";

            // ==============================
            // 2. Lấy toàn bộ câu hỏi giống HocAll
            // ==============================
            var allQuestions = _context.CauHoiLyThuyets
                .Include(c => c.Chuong)
                .Include(c => c.DapAns)
                .OrderBy(c => c.Chuong.ThuTu)
                .ThenBy(c => c.IdCauHoi)
                .ToList();

            // ==============================
            // 3. Gán GlobalIndex chung
            // ==============================
            int globalIndex = 1;
            var mappedAll = allQuestions.Select(q => new
            {
                Question = q,
                GlobalIndex = globalIndex++
            }).ToList();

            // ==============================
            // 4. Lọc câu điểm liệt
            // ==============================
            var cauLiet = mappedAll
                .Where(x =>
                    x.Question.CauLiet == true &&
                    (isXeMay ? x.Question.XeMay == true : true)
                )
                .ToList();

            // ==============================
            // 5. Gom lại theo chương + chuẩn hóa dữ liệu
            // ==============================
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

            // ==============================
            // 6. Build ViewModel
            // ==============================
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

        // ==============================
        //  CHUẨN HÓA ĐƯỜNG DẪN ẢNH
        // ==============================
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
    }
}
