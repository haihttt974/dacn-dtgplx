using dacn_dtgplx.DTOs;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace dacn_dtgplx.Controllers
{
    [Route("ThueXe")]
    public class ThueXeController : Controller
    {
        private readonly DtGplxContext _context;
        private const int PageSize = 8;

        public ThueXeController(DtGplxContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.LoaiXeList = await _context.XeTapLais
                .Select(x => x.LoaiXe)
                .Distinct()
                .ToListAsync();

            var query = _context.XeTapLais.AsQueryable();

            int total = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.CurrentPage = 1;

            var xeList = await query
                .OrderBy(x => x.XeTapLaiId)
                .Take(PageSize)
                .ToListAsync();

            return View(xeList);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> Filter(
            string? search,
            string? type,
            decimal? min,
            decimal? max,
            string? sort,
            int page = 1)
        {
            var query = _context.XeTapLais.AsQueryable();

            // Tên xe
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.LoaiXe.Contains(search));
            }

            // Loại xe (so sánh đúng loại)
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(x => x.LoaiXe == type);
            }

            // Giá min / max
            if (min.HasValue)
                query = query.Where(x => x.GiaThueTheoGio >= min);

            if (max.HasValue)
                query = query.Where(x => x.GiaThueTheoGio <= max);

            // Sort
            if (sort == "asc")
                query = query.OrderBy(x => x.GiaThueTheoGio);
            else if (sort == "desc")
                query = query.OrderByDescending(x => x.GiaThueTheoGio);
            else
                query = query.OrderBy(x => x.XeTapLaiId);

            // Paging
            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)total / PageSize);
            if (totalPages == 0) totalPages = 1;

            page = Math.Max(1, Math.Min(page, totalPages));

            var list = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return PartialView("_XeCards", list);
        }

        [HttpGet("CheckTime")]
        public async Task<IActionResult> CheckTime(int xeId, DateTime rentStart, int durationHours)
        {
            if (durationHours < 1 || durationHours > 8)
            {
                return Json(new
                {
                    success = false,
                    message = "Số giờ thuê phải từ 1 đến 8."
                });
            }

            var rentEnd = rentStart.AddHours(durationHours);

            // ----- LỊCH HỌC -----
            var date = DateOnly.FromDateTime(rentStart.Date);

            var lessons = await _context.LichHocs
                .Include(l => l.KhoaHoc)
                .Where(l =>
                    l.XeTapLaiId == xeId &&
                    l.KhoaHoc.IsActive == true &&
                    l.NgayHoc == date)
                .Select(l => new { l.TgBatDau, l.TgKetThuc })
                .ToListAsync();

            foreach (var l in lessons)
            {
                var busyStart = date.ToDateTime(l.TgBatDau);
                var busyEnd = date.ToDateTime(l.TgKetThuc);

                var protectedStart = busyStart.AddHours(-1);
                var protectedEnd = busyEnd.AddHours(1);

                if (rentStart < protectedEnd && rentEnd > protectedStart)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Xe đang có lịch học từ {busyStart:HH\\:mm} đến {busyEnd:HH\\:mm}. " +
                                  "Vui lòng chọn khung giờ cách trước/sau ít nhất 1 giờ."
                    });
                }
            }

            // ----- CÁC PHIẾU THUÊ XE ĐÃ THANH TOÁN -----
            var rentals = await _context.PhieuThueXe
                .Where(p =>
                    p.XeId == xeId &&
                    p.TgBatDau.HasValue &&
                    p.TgThue.HasValue &&
                    p.HoaDonThanhToans.Any(h => h.TrangThai == true))
                .Select(p => new { p.TgBatDau, p.TgThue })
                .ToListAsync();

            foreach (var p in rentals)
            {
                var busyStart = p.TgBatDau!.Value;
                var busyEnd = busyStart.AddHours(p.TgThue!.Value);

                var protectedStart = busyStart.AddHours(-1);
                var protectedEnd = busyEnd.AddHours(1);

                if (rentStart < protectedEnd && rentEnd > protectedStart)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Xe đã được thuê từ {busyStart:dd/MM/yyyy HH\\:mm} " +
                                  $"đến {busyEnd:dd/MM/yyyy HH\\:mm}. " +
                                  "Vui lòng chọn khung giờ cách trước/sau ít nhất 1 giờ."
                    });
                }
            }

            // OK
            return Json(new { success = true });
        }

        // ==============================
        //  AJAX: Nhấn nút Thuê xe
        // ==============================
        [HttpGet("Thue/{id}")]
        public async Task<IActionResult> Thue(int id)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null)
                return Json(new { success = false, message = "Xe không tồn tại!" });

            bool isLogged = User.Identity?.IsAuthenticated ?? false;

            if (!isLogged)
            {
                // khách → show modal nhập thông tin
                return Json(new { success = true, requireLogin = false, xeId = id });
            }

            // Đăng nhập → tự động fill form
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);

            return Json(new
            {
                success = true,
                requireLogin = true,
                xeId = id,
                userInfo = new
                {
                    ten = user.TenDayDu,
                    email = user.Email,
                    sdt = user.SoDienThoai,
                    cccd = user.Cccd
                }
            });
        }

        // ==============================
        //  Lưu tạm thông tin khách vãng lai
        // ==============================
        [HttpPost("LuuThongTinTam")]
        public IActionResult LuuThongTinTam(ThongTinThueXeDTO dto)
        {
            HttpContext.Session.SetString("RentInfo", JsonSerializer.Serialize(dto));
            //HttpContext.Session.SetString("rent_email", dto.Email);
            //HttpContext.Session.SetString("rent_name", dto.Ten);
            return Json(new { success = true });
        }

        [HttpGet("XacNhanThue")]
        public async Task<IActionResult> XacNhanThue(int id)
        {
            var xe = await _context.XeTapLais.FindAsync(id);
            if (xe == null) return NotFound();

            ThongTinThueXeDTO? info = null;

            // Nếu là khách → lấy tất cả thông tin từ Session
            if (!User.Identity!.IsAuthenticated)
            {
                var json = HttpContext.Session.GetString("RentInfo");
                if (json != null)
                    info = JsonSerializer.Deserialize<ThongTinThueXeDTO>(json);

                // Kiểm tra xem xeId trong session có khớp không
                if (info != null && info.XeId != id)
                {
                    TempData["Error"] = "Dữ liệu thuê xe không khớp.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Nếu user đã login → tự fetch thông tin
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                info = new ThongTinThueXeDTO
                {
                    Ten = user!.TenDayDu,
                    Email = user.Email,
                    SDT = user.SoDienThoai,
                    CCCD = user.Cccd,

                    // ⭐ Người đăng nhập nhưng vẫn phải nhập lại giờ thuê
                    XeId = id,
                    RentStart = DateTime.Now,    // tạm, người dùng sẽ chọn lại trong view
                    Duration = 1
                };
            }

            ViewBag.Info = info;

            return View("XacNhanThue", xe);
        }

        // ==============================
        //  POST: Tiến hành tạo phiếu + hóa đơn
        // ==============================
        [HttpPost("XacNhanThue")]
        public async Task<IActionResult> XacNhanThue(int xeId, DateTime rentStart, int duration)
        {
            int userId;

            if (User.Identity!.IsAuthenticated)
            {
                // user thật đang đăng nhập
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }
            else
            {
                // 🔥 khách vãng lai → gán vào user "guest_rent"
                var guest = await _context.Users
                    .FirstAsync(u => u.Username == "guest_rent");
                userId = guest.UserId;

                var json = HttpContext.Session.GetString("RentInfo");
                if (json != null)
                {
                    var info = JsonSerializer.Deserialize<ThongTinThueXeDTO>(json);
                    if (info != null)
                    {
                        HttpContext.Session.SetString("rent_email", info.Email ?? "");
                        HttpContext.Session.SetString("rent_name", info.Ten ?? "");
                    }
                }
            }

            var phieu = new PhieuThueXe
            {
                UserId = userId,
                XeId = xeId,
                TgBatDau = rentStart,
                TgThue = duration
            };

            _context.PhieuThueXe.Add(phieu);
            await _context.SaveChangesAsync();

            var xe = await _context.XeTapLais.FindAsync(xeId);
            decimal total = (xe!.GiaThueTheoGio ?? 0) * duration;

            var hd = new HoaDonThanhToan
            {
                PhieuTxId = phieu.PhieuTxId,
                SoTien = total,
                TrangThai = null
            };

            _context.HoaDonThanhToans.Add(hd);
            await _context.SaveChangesAsync();

            return RedirectToAction("StartPayment", "PaymentRent", new { hoaDonId = hd.IdThanhToan });
        }
    }
}
