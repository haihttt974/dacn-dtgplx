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

            int total = await _context.XeTapLais.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / PageSize);

            var xeList = await _context.XeTapLais
                .Take(PageSize)
                .ToListAsync();

            return View(xeList);
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
