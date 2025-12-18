using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace dacn_dtgplx.Controllers
{
    public class AdminVehiclesController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly QrCryptoService _qrCryptoService;

        public AdminVehiclesController(DtGplxContext context, QrCryptoService qrCryptoService)
        {
            _context = context;
            _qrCryptoService = qrCryptoService;
        }

        // ======================= INDEX =========================
        public async Task<IActionResult> Index()
        {
            var data = await _context.XeTapLais
                .OrderBy(x => x.XeTapLaiId)
                .ToListAsync();

            return View(data);
        }

        // ======================= CREATE =========================
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(XeTapLai model)
        {
            if (string.IsNullOrWhiteSpace(model.LoaiXe))
                ModelState.AddModelError(nameof(XeTapLai.LoaiXe), "Tên xe không được để trống.");

            if (!ModelState.IsValid)
                return View(model);

            _context.XeTapLais.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm xe thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ======================= EDIT =========================
        public async Task<IActionResult> Edit(int id)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();
            return View(xe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, XeTapLai model)
        {
            if (id != model.XeTapLaiId) return NotFound();

            if (string.IsNullOrWhiteSpace(model.LoaiXe))
                ModelState.AddModelError(nameof(XeTapLai.LoaiXe), "Tên xe không được để trống.");

            if (!ModelState.IsValid)
                return View(model);

            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();

            xe.LoaiXe = model.LoaiXe;
            xe.TrangThaiXe = model.TrangThaiXe;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật xe thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ======================= DELETE =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();

            _context.XeTapLais.Remove(xe);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa xe thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ======================= DETAILS (TRANG GỐC) =========================
        public async Task<IActionResult> Details(int id)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();

            var vm = new VehicleScheduleVM
            {
                XeId = id,
                LoaiXe = xe.LoaiXe,
                TrangThaiXe = xe.TrangThaiXe,
                Mode = "week"
            };

            await LoadWeekData(vm, DateTime.Today);

            return View(vm);
        }

        // ============================================================== 
        // =============== AJAX LOAD WEEK VIEW ========================== 
        // ============================================================== 
        [HttpGet]
        public async Task<IActionResult> LoadWeek(int id, DateTime? date)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();

            var vm = new VehicleScheduleVM
            {
                XeId = id,
                LoaiXe = xe.LoaiXe,
                TrangThaiXe = xe.TrangThaiXe,
                Mode = "week"
            };

            await LoadWeekData(vm, date ?? DateTime.Today);

            return PartialView("_WeekView", vm);
        }

        private async Task LoadWeekData(VehicleScheduleVM vm, DateTime date)
        {
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var weekStart = date.Date.AddDays(-diff);
            var weekEnd = weekStart.AddDays(6);

            vm.WeekStart = weekStart;
            vm.WeekEnd = weekEnd;
            vm.WeekDays = Enumerable.Range(0, 7)
                .Select(i => weekStart.AddDays(i))
                .ToList();

            vm.TimeSlots = CreateTimeSlots();

            var lichHoc = await _context.LichHocs
                .Include(l => l.KhoaHoc)
                .Where(l => l.XeTapLaiId == vm.XeId &&
                            l.NgayHoc >= DateOnly.FromDateTime(weekStart) &&
                            l.NgayHoc <= DateOnly.FromDateTime(weekEnd))
                .ToListAsync();

            foreach (var lh in lichHoc)
            {
                vm.Items.Add(new VehicleScheduleItemVM
                {
                    Date = lh.NgayHoc,
                    Start = lh.TgBatDau,
                    End = lh.TgKetThuc,
                    Type = "LichHoc",
                    Title = lh.KhoaHoc?.TenKhoaHoc ?? "Lịch học",
                    Description = $"{lh.NoiDung} - {lh.DiaDiem}"
                });
            }

            var phieu = await _context.PhieuThueXe
                .Include(p => p.User)
                .Where(p => p.XeId == vm.XeId &&
                            p.TgBatDau != null &&
                            p.TgThue != null)
                .ToListAsync();

            foreach (var p in phieu)
            {
                var s = p.TgBatDau!.Value;
                var e = s.AddHours(p.TgThue ?? 0);

                if (e.Date < weekStart || s.Date > weekEnd)
                    continue;

                vm.Items.Add(new VehicleScheduleItemVM
                {
                    Date = DateOnly.FromDateTime(s),
                    Start = TimeOnly.FromDateTime(s),
                    End = TimeOnly.FromDateTime(e),
                    Type = "PhieuThue",
                    Title = $"Thuê xe - {p.User.Username}",
                    Description = $"{s:HH:mm} - {e:HH:mm}"
                });
            }
        }

        // ============================================================== 
        // =============== AJAX LOAD MONTH VIEW ========================== 
        // ============================================================== 
        [HttpGet]
        public async Task<IActionResult> LoadMonth(int id, int month = 0, int year = 0)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();

            // ====== VALIDATE MONTH / YEAR ======
            if (month < 1 || month > 12)
                month = DateTime.Today.Month;

            if (year < 1)
                year = DateTime.Today.Year;

            var vm = new VehicleScheduleVM
            {
                XeId = id,
                LoaiXe = xe.LoaiXe,
                TrangThaiXe = xe.TrangThaiXe,
                Mode = "month",
                SelectedMonth = month,
                SelectedYear = year
            };

            // GỌI LOAD DỮ LIỆU THÁNG
            await LoadMonthData(vm, month, year);

            return PartialView("_MonthView", vm);
        }

        private async Task LoadMonthData(VehicleScheduleVM vm, int month, int year)
        {
            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var lichHoc = await _context.LichHocs
                .Include(l => l.KhoaHoc)
                .Where(l => l.XeTapLaiId == vm.XeId &&
                            l.NgayHoc >= monthStart &&
                            l.NgayHoc <= monthEnd)
                .ToListAsync();

            foreach (var lh in lichHoc)
            {
                vm.MonthItems.Add(new VehicleScheduleItemVM
                {
                    Date = lh.NgayHoc,
                    Start = lh.TgBatDau,
                    End = lh.TgKetThuc,
                    Type = "LichHoc",
                    Title = lh.KhoaHoc?.TenKhoaHoc ?? "Lịch học",
                    Description = $"{lh.NoiDung} - {lh.DiaDiem}"
                });
            }

            var phieu = await _context.PhieuThueXe
                .Include(p => p.User)
                .Where(p => p.XeId == vm.XeId &&
                            p.TgBatDau != null &&
                            p.TgThue != null)
                .ToListAsync();

            foreach (var p in phieu)
            {
                var s = p.TgBatDau!.Value;
                var e = s.AddHours(p.TgThue ?? 0);

                if (s.Month != month || s.Year != year)
                    continue;

                vm.MonthItems.Add(new VehicleScheduleItemVM
                {
                    Date = DateOnly.FromDateTime(s),
                    Start = TimeOnly.FromDateTime(s),
                    End = TimeOnly.FromDateTime(e),
                    Type = "PhieuThue",
                    Title = $"Thuê xe - {p.User.Username}",
                    Description = $"{s:HH:mm} - {e:HH:mm}"
                });
            }
        }

        private List<TimeSlotVM> CreateTimeSlots()
        {
            var list = new List<TimeSlotVM>();
            int index = 1;

            for (int h = 0; h < 24; h += 2)
            {
                list.Add(new TimeSlotVM
                {
                    Index = index++,
                    Start = new TimeOnly(h, 0),
                    End = new TimeOnly(Math.Min(h + 2, 23), 59),
                    Label = $"{h:00}:00 - {Math.Min(h + 2, 24):00}:00"
                });
            }

            return list;
        }

        // ======================= SCAN QR =========================
        public IActionResult Scan()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ScanResult(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Mã QR trống" });

            JsonElement payload;

            try
            {
                // 1. Giải mã QR
                string json = _qrCryptoService.Decrypt(code);

                payload = JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch
            {
                // Sai key / bị sửa / QR giả
                return Json(new { success = false, message = "Mã QR không hợp lệ hoặc đã bị chỉnh sửa" });
            }

            // 2. Validate payload bắt buộc
            if (!payload.TryGetProperty("paymentId", out var paymentIdProp)
                || !paymentIdProp.TryGetInt32(out int paymentId))
            {
                return Json(new { success = false, message = "QR không hợp lệ" });
            }

            // (khuyến nghị) validate type
            if (!payload.TryGetProperty("type", out var typeProp)
                || typeProp.GetString() != "RENT_PAYMENT")
            {
                return Json(new { success = false, message = "QR sai loại" });
            }

            // (khuyến nghị) validate thời gian
            if (payload.TryGetProperty("ts", out var tsProp))
            {
                long ts = tsProp.GetInt64();
                var qrTime = DateTimeOffset.FromUnixTimeSeconds(ts);

                if (DateTimeOffset.UtcNow - qrTime > TimeSpan.FromMinutes(10))
                {
                    return Json(new { success = false, message = "QR đã hết hạn" });
                }
            }

            // 3. Lấy hóa đơn
            var bill = await _context.HoaDonThanhToans
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.Xe)
                .Include(h => h.PhieuTx)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(h => h.IdThanhToan == paymentId);

            if (bill == null)
                return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

            // 4. Render partial
            string html = await this.RenderViewAsync("_ScanBillDetail", bill, true);

            return Json(new
            {
                success = true,
                html,
                valid = bill.TrangThai == true
            });
        }
    }
}
