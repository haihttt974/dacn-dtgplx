using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class AdminSignsController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminSignsController(DtGplxContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ===================== DANH SÁCH BIỂN BÁO =====================
        public async Task<IActionResult> Index()
        {
            var signs = await _context.BienBaos
                .OrderBy(x => x.IdBienBao)
                .ToListAsync();

            return View(signs);
        }

        // ===================== CREATE =====================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BienBao model, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(model.TenBienBao))
            {
                ModelState.AddModelError(nameof(BienBao.TenBienBao), "Tên biển báo không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Xử lý upload ảnh nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("imageFile", "Chỉ cho phép ảnh định dạng JPG, PNG, GIF, WEBP.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "signs");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối vào DB
                model.HinhAnh = $"/uploads/signs/{fileName}";
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm biển báo thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ===================== EDIT =====================
        public async Task<IActionResult> Edit(int id)
        {
            var sign = await _context.BienBaos.FindAsync(id);
            if (sign == null) return NotFound();

            return View(sign);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BienBao model, IFormFile? imageFile)
        {
            if (id != model.IdBienBao) return NotFound();

            if (string.IsNullOrWhiteSpace(model.TenBienBao))
            {
                ModelState.AddModelError(nameof(BienBao.TenBienBao), "Tên biển báo không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var sign = await _context.BienBaos.FindAsync(id);
            if (sign == null) return NotFound();

            // Cập nhật thông tin
            sign.TenBienBao = model.TenBienBao;
            sign.Ynghia = model.Ynghia;

            // Nếu có upload ảnh mới
            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("imageFile", "Chỉ cho phép ảnh định dạng JPG, PNG, GIF, WEBP.");
                    return View(model);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(sign.HinhAnh))
                {
                    var oldPath = Path.Combine(
                        _env.WebRootPath,
                        sign.HinhAnh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "signs");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                sign.HinhAnh = $"/uploads/signs/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật biển báo thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ===================== DELETE =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sign = await _context.BienBaos.FindAsync(id);
            if (sign == null) return NotFound();

            // TODO: nếu đã có FlashCard tham chiếu thì có thể cần check trước khi xóa

            // Xóa file ảnh trên ổ đĩa
            if (!string.IsNullOrEmpty(sign.HinhAnh))
            {
                var oldPath = Path.Combine(
                    _env.WebRootPath,
                    sign.HinhAnh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            _context.BienBaos.Remove(sign);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa biển báo thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}