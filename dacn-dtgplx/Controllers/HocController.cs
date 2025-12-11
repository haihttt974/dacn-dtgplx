using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        public IActionResult ChonHang(string maHang)
        {
            if (!string.IsNullOrEmpty(maHang))
                HttpContext.Session.SetString("Hang", maHang);

            return RedirectToAction("Index");
        }
    }
}
