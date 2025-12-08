using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class LyThuyetController : Controller
    {
        private readonly DtGplxContext _context;

        public LyThuyetController(DtGplxContext context)
        {
            _context = context;
        }

        // ======================================================
        // 👉 INDEX: Danh sách bộ đề của hạng đang chọn
        // ======================================================
        public IActionResult Index()
        {
            var selectedHang = HttpContext.Session.GetString("Hang");
            // lấy user
            var userId = HttpContext.Session.GetInt32("UserId");

            // lấy bài làm gần nhất của user cho từng bộ đề
            var baiLamDict = _context.BaiLams
                .Where(b => b.UserId == userId)
                .GroupBy(b => b.IdBoDe)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.BaiLamId).First());

            ViewBag.BaiLamDict = baiLamDict;

            if (string.IsNullOrEmpty(selectedHang))
                return RedirectToAction("Index", "Hoc"); // bắt buộc chọn hạng

            // Tìm thông tin hạng
            var hang = _context.Hangs.FirstOrDefault(h => h.MaHang == selectedHang);

            if (hang == null)
            {
                TempData["Error"] = "Không tìm thấy hạng GPLX.";
                return RedirectToAction("Index", "Hoc");
            }

            // Lấy danh sách bộ đề đang hoạt động
            var dsBoDe = _context.BoDeThiThus
                .Where(b => b.IdHang == hang.IdHang && b.HoatDong == true)
                .OrderBy(b => b.IdBoDe)
                .ToList();

            // Lấy bài làm của user (nếu có)
            if (userId != null)
            {
                ViewBag.BaiLamDict = _context.BaiLams
                    .Where(b => b.UserId == userId)
                    .GroupBy(b => b.IdBoDe)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.BaiLamId).First());
            }
            else
            {
                ViewBag.BaiLamDict = new Dictionary<int, BaiLam>();
            }

            return View(dsBoDe);
        }

        // ======================================================
        // 👉 GET: Exam – vào phòng thi
        // ======================================================
        [HttpGet]
        public IActionResult Exam(int idBoDe, bool history = false)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Nếu là xem lịch sử -> load bài làm từ DB
            if (history && userId != null)
            {
                var baiLam = _context.BaiLams
                    .Include(b => b.ChiTietBaiLams)
                    .FirstOrDefault(b => b.IdBoDe == idBoDe && b.UserId == userId);

                if (baiLam != null)
                {
                    // Build ViewModel từ kết quả trong DB
                    var vm = BuildHistoryViewModel(idBoDe, baiLam);
                    return View(vm);
                }
            }

            // Nếu KHÔNG xem lịch sử -> thi bình thường
            var examVm = BuildExamViewModel(idBoDe);
            if (examVm == null)
            {
                TempData["Error"] = "Bộ đề không tồn tại hoặc không khả dụng.";
                return RedirectToAction("Index");
            }

            return View(examVm);
        }

        private ExamViewModel BuildHistoryViewModel(int idBoDe, BaiLam baiLam)
        {
            var vm = BuildExamViewModel(idBoDe);
            vm.IsSubmitted = true;         // CHẾ ĐỘ XEM LỊCH SỬ
            vm.ThoiGianLam = (int)baiLam.ThoiGianLamBai;
            vm.SoCauSai = (int)baiLam.SoCauSai;
            vm.SoCauDung = (int)(vm.TongCau - baiLam.SoCauSai);
            vm.Dat = baiLam.KetQua ?? false;

            // Gán kết quả người làm
            foreach (var ct in baiLam.ChiTietBaiLams)
            {
                if (int.TryParse(ct.DapAnDaChon, out int ans))
                    vm.DapAnDaChon[ct.IdCauHoi] = ans;
                else
                    vm.DapAnDaChon[ct.IdCauHoi] = null;
            }

            return vm;
        }

        // ======================================================
        // 👉 POST: Exam – Nộp bài
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Exam(int idBoDe, int timeLeftSeconds)
        {
            var vm = BuildExamViewModel(idBoDe);
            if (vm == null)
            {
                TempData["Error"] = "Bộ đề không tồn tại hoặc không khả dụng.";
                return RedirectToAction("Index");
            }

            // tổng thời gian cho phép (giây)
            int totalSeconds = vm.ThoiGian * 60;
            if (totalSeconds <= 0) totalSeconds = 20 * 60;

            int usedSeconds = totalSeconds - timeLeftSeconds;
            if (usedSeconds < 0) usedSeconds = 0;
            if (usedSeconds > totalSeconds) usedSeconds = totalSeconds;

            vm.ThoiGianLam = usedSeconds;
            vm.IsSubmitted = true;

            // lấy câu trả lời từ form
            foreach (var q in vm.CauHoi)
            {
                string key = $"answer_{q.IdCauHoi}";
                var value = Request.Form[key];

                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var ansId))
                {
                    vm.DapAnDaChon[q.IdCauHoi] = ansId;
                }
                else
                {
                    vm.DapAnDaChon[q.IdCauHoi] = null;
                }
            }

            // chấm điểm
            int correct = 0;
            int wrong = 0;
            bool cauLietSai = false;

            foreach (var q in vm.CauHoi)
            {
                var correctAnswer = q.DapAn.FirstOrDefault(a => a.IsCorrect);
                vm.DapAnDaChon.TryGetValue(q.IdCauHoi, out int? userAnsId);

                bool isCorrect = correctAnswer != null &&
                                 userAnsId.HasValue &&
                                 userAnsId.Value == correctAnswer.IdDapAn;

                if (isCorrect)
                {
                    correct++;
                }
                else if (userAnsId.HasValue)
                {
                    wrong++;
                    if (q.LaCauLiet)
                        cauLietSai = true;
                }
            }

            vm.SoCauDung = correct;
            vm.SoCauSai = vm.TongCau - correct;
            vm.CoCauLietSai = cauLietSai;
            vm.Dat = (correct >= vm.DiemDat) && !cauLietSai;

            // Nếu user đã đăng nhập -> lưu vào DB
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                SaveExamResultToDatabase(userId.Value, vm);
            }

            // trả lại cùng View Exam nhưng ở trạng thái review
            return View(vm);
        }

        // ======================================================
        // Hàm build ViewModel dùng chung cho cả GET & POST
        // ======================================================
        private ExamViewModel? BuildExamViewModel(int idBoDe)
        {
            var selectedHang = HttpContext.Session.GetString("Hang");
            if (string.IsNullOrEmpty(selectedHang))
                return null;

            var boDe = _context.BoDeThiThus
                .Include(b => b.IdHangNavigation)
                .Include(b => b.ChiTietBoDeTns)
                    .ThenInclude(ct => ct.IdCauHoiNavigation)
                        .ThenInclude(ch => ch.DapAns)
                .FirstOrDefault(b => b.IdBoDe == idBoDe && b.HoatDong == true);

            if (boDe == null)
                return null;

            // kiểm tra hạng
            var hang = boDe.IdHangNavigation;
            if (hang.MaHang.Trim().ToUpper() != selectedHang.Trim().ToUpper())
                return null;

            int thoiGian = boDe.ThoiGian ?? hang.ThoiGianTn;
            if (thoiGian <= 0) thoiGian = 20;

            var vm = new ExamViewModel
            {
                IdBoDe = boDe.IdBoDe,
                TenBoDe = boDe.TenBoDe ?? $"Bộ đề {boDe.IdBoDe}",
                Hang = hang.MaHang,
                ThoiGian = thoiGian,
                TongCau = boDe.SoCauHoi ?? boDe.ChiTietBoDeTns.Count,
                DiemDat = hang.DiemDat
            };

            var chiTietOrdered = boDe.ChiTietBoDeTns
                .OrderBy(ct => ct.ThuTu ?? int.MaxValue)
                .ToList();

            foreach (var ct in chiTietOrdered)
            {
                var ch = ct.IdCauHoiNavigation;

                var qVm = new ExamQuestionVM
                {
                    IdCauHoi = ch.IdCauHoi,
                    NoiDung = ch.NoiDung ?? "",
                    LaCauLiet = ch.CauLiet == true,
                    ImageUrl = NormalizeImagePath(ch.HinhAnh)
                };

                // Dap an: chỉ cần Id + thứ tự. Text trong ảnh nên Label chỉ là số
                var dapAns = ch.DapAns
                    .OrderBy(d => d.ThuTu)
                    .Select((d, index) => new ExamAnswerVM
                    {
                        IdDapAn = d.IdDapAn,
                        Label = (index + 1).ToString(),
                        IsCorrect = d.DapAnDung
                    })
                    .ToList();

                // shuffle đáp án
                qVm.DapAn = dapAns.OrderBy(_ => Guid.NewGuid()).ToList();

                vm.CauHoi.Add(qVm);
            }

            return vm;
        }

        /// Chuẩn hóa đường dẫn ảnh: bỏ "wwwroot" nếu có
        private string? NormalizeImagePath(string? rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
                return null;

            var path = rawPath.Replace("\\", "/");
            if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("wwwroot/".Length);
            }

            // thêm "~/" để Razor hiểu là root
            if (!path.StartsWith("~"))
            {
                path = "~/" + path.TrimStart('/');
            }

            return path;
        }

        private void SaveExamResultToDatabase(int userId, ExamViewModel vm)
        {
            var baiLam = new BaiLam
            {
                UserId = userId,
                IdBoDe = vm.IdBoDe,
                ThoiGianLamBai = vm.ThoiGianLam,
                SoCauSai = vm.SoCauSai,
                KetQua = vm.Dat,
            };

            _context.BaiLams.Add(baiLam);
            _context.SaveChanges(); // để có BaiLamId

            foreach (var q in vm.CauHoi)
            {
                vm.DapAnDaChon.TryGetValue(q.IdCauHoi, out int? ansId);

                var correctAnswer = q.DapAn.FirstOrDefault(a => a.IsCorrect);
                bool isCorrect = correctAnswer != null &&
                                 ansId.HasValue &&
                                 ansId.Value == correctAnswer.IdDapAn;

                var ct = new ChiTietBaiLam
                {
                    BaiLamId = baiLam.BaiLamId,
                    IdCauHoi = q.IdCauHoi,
                    DapAnDaChon = ansId?.ToString(),
                    KetQuaCau = isCorrect
                };

                _context.ChiTietBaiLams.Add(ct);
            }

            _context.SaveChanges();
        }
    }
}
