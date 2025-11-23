using System.Linq;
using System.Threading.Tasks;
using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class AdminFlashCardsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminFlashCardsController(DtGplxContext context)
        {
            _context = context;
        }

        // =============== TỔNG HỢP FLASHCARD THEO BIỂN BÁO ===============
        public async Task<IActionResult> Index()
        {
            var data = await _context.BienBaos
                .Include(b => b.FlashCards)
                .OrderBy(b => b.IdBienBao)
                .Select(b => new FlashCardSignSummaryVM
                {
                    IdBienBao = b.IdBienBao,
                    TenBienBao = b.TenBienBao,
                    Ynghia = b.Ynghia,
                    HinhAnh = b.HinhAnh,
                    SoDanhGia = b.FlashCards.Count
                })
                .ToListAsync();

            return View(data);
        }

        // =============== CHI TIẾT FLASHCARD CỦA 1 BIỂN BÁO ===============
        public async Task<IActionResult> Details(int id)
        {
            var sign = await _context.BienBaos
                .Include(b => b.FlashCards)
                .ThenInclude(fc => fc.User)
                .FirstOrDefaultAsync(b => b.IdBienBao == id);

            if (sign == null) return NotFound();

            var vm = new FlashCardDetailVM
            {
                IdBienBao = sign.IdBienBao,
                TenBienBao = sign.TenBienBao,
                Ynghia = sign.Ynghia,
                HinhAnh = sign.HinhAnh,
                Items = sign.FlashCards
                    .OrderByDescending(fc => fc.IdFlashcard)
                    .Select(fc => new FlashCardItemVM
                    {
                        IdFlashcard = fc.IdFlashcard,
                        DanhGia = fc.DanhGia,
                        UserId = fc.UserId,
                        UserName = fc.User.Username   // sửa nếu tên cột khác
                    })
                    .ToList()
            };

            return View(vm);
        }

        // =============== XOÁ 1 FLASHCARD ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int signId)
        {
            var flash = await _context.FlashCards.FindAsync(id);
            if (flash == null) return NotFound();

            _context.FlashCards.Remove(flash);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa đánh giá (Flashcard) thành công.";
            return RedirectToAction(nameof(Details), new { id = signId });
        }
    }
}
