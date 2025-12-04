using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    [Authorize]
    public class HoSoThiSinhController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly ISteganographyService _steg;

        public HoSoThiSinhController(
            DtGplxContext context,
            ISteganographyService steg)
        {
            _context = context;
            _steg = steg;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ======================================================
        // CREATE (GET)
        // ======================================================
        public async Task<IActionResult> Create()
        {
            ViewBag.Hangs = await _context.Hangs.ToListAsync();
            return View(new HoSoThiSinhCreateVM());
        }

        // ======================================================
        // CREATE (POST)
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoSoThiSinhCreateVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Hangs = await _context.Hangs.ToListAsync();
                    TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                    return View(model);
                }

                int userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    ViewBag.Hangs = await _context.Hangs.ToListAsync();
                    return View(model);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // 1) Lưu ảnh giấy khám
                List<string> healthImages = new();
                int stt = 1;

                foreach (var img in model.AnhGiayKham)
                {
                    if (img == null || img.Length == 0)
                        continue;

                    string relative = $"images/healths/{timestamp}_{userId}_{stt}.png";
                    string fullPath = Path.Combine("wwwroot", relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                    using (var fs = new FileStream(fullPath, FileMode.Create))
                    {
                        await img.CopyToAsync(fs);
                    }

                    healthImages.Add(relative.Replace("\\", "/"));
                    stt++;
                }

                // 2) Build JSON sức khỏe
                var jsonObj = new Dictionary<string, object?>
                {
                    ["anh_giay_kham"] = healthImages,
                    ["thoi_han"] = model.SucKhoe.thoi_han?.ToString("yyyy-MM-dd"),
                    ["mat"] = new Dictionary<string, string?>
                    {
                        ["mat_trai(10)"] = model.SucKhoe.mat.mat_trai?.ToString(),
                        ["mat_phai(10)"] = model.SucKhoe.mat.mat_phai?.ToString()
                    },
                    ["huyet_ap(120)"] = model.SucKhoe.huyet_ap,
                    ["chieu_cao (cm)"] = model.SucKhoe.chieu_cao?.ToString(),
                    ["can_nang (kg)"] = model.SucKhoe.can_nang?.ToString()
                };

                string jsonText = JsonSerializer.Serialize(jsonObj);

                // 3) Giấu JSON vào ảnh thẻ
                string idPhotoRelative = $"images/idPhoto/{timestamp}_{userId}.png";

                string savedIdPhoto = await _steg.HideJsonIntoImageAsync(
                    model.AnhThe,
                    idPhotoRelative,
                    jsonText
                );

                // 4) Lưu hồ sơ
                var hang = await _context.Hangs
                    .FirstOrDefaultAsync(h => h.IdHang == model.IdHang);

                if (hang == null)
                {
                    TempData["Error"] = "Không tìm thấy hạng GPLX.";
                    ViewBag.Hangs = await _context.Hangs.ToListAsync();
                    return View(model);
                }

                var hoSo = new HoSoThiSinh
                {
                    UserId = userId,
                    KhamSucKhoe = savedIdPhoto,
                    NgayDk = DateOnly.FromDateTime(DateTime.Now),
                    GhiChu = null,
                    DaDuyet = null,
                    LoaiHoSo = $"Hồ sơ {hang.MaHang} - {user.TenDayDu}"
                };

                _context.HoSoThiSinhs.Add(hoSo);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tạo hồ sơ thành công!";
                return RedirectToAction(nameof(MyProfile));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                ViewBag.Hangs = await _context.Hangs.ToListAsync();
                return View(model);
            }
        }

        // ======================================================
        // MY PROFILE
        // ======================================================
        public async Task<IActionResult> MyProfile(bool? daDuyet, int? idHang)
        {
            int userId = GetUserId();

            var query = _context.HoSoThiSinhs
                .Include(h => h.User)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.NgayDk)
                .AsQueryable();

            if (daDuyet.HasValue)
            {
                query = query.Where(h => h.DaDuyet == daDuyet.Value);
            }

            if (idHang.HasValue)
            {
                var hang = await _context.Hangs.FindAsync(idHang.Value);
                if (hang != null)
                {
                    query = query.Where(h => h.LoaiHoSo.Contains(hang.MaHang));
                }
            }

            ViewBag.Hangs = await _context.Hangs.ToListAsync();
            ViewBag.FilterDaDuyet = daDuyet;
            ViewBag.FilterIdHang = idHang;

            var list = await query.ToListAsync();
            return View(list);
        }

        // ======================================================
        // DETAIL
        // ======================================================
        public async Task<IActionResult> Detail(int id)
        {
            int userId = GetUserId();

            var hoSo = await _context.HoSoThiSinhs
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.HoSoId == id && h.UserId == userId);

            if (hoSo == null)
                return NotFound();

            HealthInfoVM? sucKhoe = null;
            string? rawJson = null;

            if (!string.IsNullOrEmpty(hoSo.KhamSucKhoe))
            {
                rawJson = await _steg.ExtractJsonFromImageAsync(hoSo.KhamSucKhoe);
                if (!string.IsNullOrWhiteSpace(rawJson))
                {
                    try
                    {
                        sucKhoe = JsonSerializer.Deserialize<HealthInfoVM>(rawJson);
                    }
                    catch
                    {
                        sucKhoe = null;
                    }
                }
            }

            var vm = new DetailHoSoVM
            {
                HoSo = hoSo,
                RawJson = rawJson,
                SucKhoe = sucKhoe
            };

            return View(vm);
        }
    }
}
