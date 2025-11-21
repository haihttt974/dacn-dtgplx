using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class AdminExamSetsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminExamSetsController(DtGplxContext context)
        {
            _context = context;
        }

        // ================== HELPER LOAD DROPDOWNS ==================
        private async Task LoadDropdownData(ExamSetEditViewModel model)
        {
            model.Hangs = await _context.Hangs
                .OrderBy(h => h.MaHang)
                .ToListAsync();

            model.Chuongs = await _context.Chuongs
                .Include(ch => ch.CauHoiLyThuyets)
                .OrderBy(ch => ch.ThuTu ?? ch.ChuongId)
                .ToListAsync();

            model.AllQuestions = await _context.CauHoiLyThuyets
                .Include(c => c.Chuong)
                .OrderBy(c => c.IdCauHoi)
                .ToListAsync();
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index(int? idHang)
        {
            var query = _context.BoDeThiThus
                .Include(b => b.IdHangNavigation)
                .AsQueryable();

            if (idHang.HasValue)
                query = query.Where(b => b.IdHang == idHang.Value);

            var boDes = await query.OrderByDescending(b => b.TaoLuc).ToListAsync();

            ViewBag.Hangs = await _context.Hangs.OrderBy(h => h.MaHang).ToListAsync();

            return View(boDes);
        }

        // ================== CREATE (GET) ==================
        public async Task<IActionResult> Create()
        {
            var vm = new ExamSetEditViewModel
            {
                HoatDong = true
            };

            // Load danh sách hạng + câu hỏi cho View
            await LoadDropdownData(vm);

            return View(vm);
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamSetEditViewModel model)
        {
            // Lấy thông tin hạng
            var hang = await _context.Hangs.FirstOrDefaultAsync(h => h.IdHang == model.IdHang);
            if (hang == null)
            {
                ModelState.AddModelError("IdHang", "Không tìm thấy hạng GPLX.");
                await LoadDropdownData(model);
                return View(model);
            }

            // Auto generate tên bộ đề
            var soDeHienCo = await _context.BoDeThiThus.CountAsync(b => b.IdHang == hang.IdHang);
            string tenBoDe = $"Đề chuẩn hạng {hang.MaHang} {soDeHienCo + 1}";

            // Thời gian = giây → phút
            int thoiGianPhut = hang.ThoiGianTn / 60;

            // Số câu tối đa theo hạng
            int soCauToiDa = hang.SoCauHoi;

            // =========== RANDOM CÂU HỎI THEO TỈ LỆ ===========

            // Load toàn bộ câu hỏi & chương
            var cauHoi = await _context.CauHoiLyThuyets
                .Include(c => c.Chuong)
                .ToListAsync();

            // TỈ LỆ THEO HẠNG
            Dictionary<int, double> tile = new();

            if (hang.MaHang == "A" || hang.MaHang == "A1")
            {
                tile = new Dictionary<int, double>
        {
            {1, 39.2/100}, {2, 3.2/100}, {3, 3.2/100},
            {4, 0}, {5, 36.0/100}, {6, 18.4/100}
        };
            }
            else
            {
                tile = new Dictionary<int, double>
        {
            {1, 30.83/100}, {2, 4.17/100}, {3, 12.5/100},
            {4, 13.33/100}, {5, 16.67/100}, {6, 22.50/100}
        };
            }

            // TÍNH SỐ CÂU THEO CHƯƠNG
            var rand = new Random();
            var cauHoiTheoChuong = new Dictionary<int, List<CauHoiLyThuyet>>();

            foreach (var ch in tile.Keys)
            {
                cauHoiTheoChuong[ch] = cauHoi.Where(c => c.ChuongId == ch).ToList();
            }

            List<int> selectedIds = new();
            int tongDaLay = 0;

            foreach (var pair in tile)
            {
                int chuongId = pair.Key;
                double tyLe = pair.Value;

                int soCau = (int)Math.Round(soCauToiDa * tyLe);

                tongDaLay += soCau;

                var list = cauHoiTheoChuong[chuongId];

                if (list.Any())
                {
                    var randomPick = list.OrderBy(x => rand.Next()).Take(soCau).Select(x => x.IdCauHoi);
                    selectedIds.AddRange(randomPick);
                }
            }

            // Nếu tổng < số câu tối đa → bổ sung random
            if (selectedIds.Count < soCauToiDa)
            {
                var conLai = soCauToiDa - selectedIds.Count;

                var remaining = cauHoi
                    .Where(x => !selectedIds.Contains(x.IdCauHoi))
                    .OrderBy(x => rand.Next())
                    .Take(conLai)
                    .Select(x => x.IdCauHoi);

                selectedIds.AddRange(remaining);
            }

            // =========== LƯU BỘ ĐỀ ===========

            var boDe = new BoDeThiThu
            {
                TenBoDe = tenBoDe,
                ThoiGian = thoiGianPhut,
                SoCauHoi = soCauToiDa,
                HoatDong = true,
                TaoLuc = DateTime.Now,
                IdHang = hang.IdHang
            };

            int thuTu = 1;
            foreach (var idCauHoi in selectedIds)
            {
                boDe.ChiTietBoDeTns.Add(new ChiTietBoDeTn
                {
                    IdCauHoi = idCauHoi,
                    ThuTu = thuTu++
                });
            }

            _context.BoDeThiThus.Add(boDe);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm mới bộ đề thành công thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT (GET) ==================
        public async Task<IActionResult> Edit(int id)
        {
            var boDe = await _context.BoDeThiThus
                .Include(b => b.IdHangNavigation)
                .Include(b => b.ChiTietBoDeTns)
                    .ThenInclude(ct => ct.IdCauHoiNavigation)
                .FirstOrDefaultAsync(b => b.IdBoDe == id);

            if (boDe == null) return NotFound();

            var vm = new ExamSetEditViewModel
            {
                IdBoDe = boDe.IdBoDe,
                TenBoDe = boDe.TenBoDe,
                IdHang = boDe.IdHang,
                ThoiGian = boDe.ThoiGian,
                // số câu hiện có trong bộ đề = số chi tiết
                SoCauHoi = boDe.ChiTietBoDeTns.Count,
                HoatDong = boDe.HoatDong ?? false,
                SelectedQuestionIds = boDe.ChiTietBoDeTns
                    .OrderBy(ct => ct.ThuTu)
                    .Select(ct => ct.IdCauHoi)
                    .ToList(),
                // 🔹 số câu tối đa theo hạng:
                MaxQuestions = boDe.IdHangNavigation?.SoCauHoi
            };

            await LoadDropdownData(vm);
            return View(vm);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamSetEditViewModel model)
        {
            if (id != model.IdBoDe) return NotFound();

            // số câu hiện có
            model.SoCauHoi = model.SelectedQuestionIds?.Count ?? 0;

            var hang = await _context.Hangs.FirstOrDefaultAsync(h => h.IdHang == model.IdHang);
            if (hang == null)
            {
                ModelState.AddModelError("IdHang", "Không tìm thấy hạng GPLX.");
            }
            else
            {
                model.MaxQuestions = hang.SoCauHoi;

                if (model.SoCauHoi > hang.SoCauHoi)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Số câu hỏi trong bộ đề ({model.SoCauHoi}) không được vượt quá số câu tối đa ({hang.SoCauHoi}) của hạng {hang.MaHang}.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(model);
                return View(model);
            }

            var boDe = await _context.BoDeThiThus
                .Include(b => b.ChiTietBoDeTns)
                .FirstOrDefaultAsync(b => b.IdBoDe == id);

            if (boDe == null) return NotFound();

            boDe.TenBoDe = model.TenBoDe;
            boDe.IdHang = model.IdHang;
            boDe.ThoiGian = model.ThoiGian;
            // lưu số câu hiện có
            boDe.SoCauHoi = model.SoCauHoi;
            boDe.HoatDong = model.HoatDong;

            _context.ChiTietBoDeTns.RemoveRange(boDe.ChiTietBoDeTns);
            boDe.ChiTietBoDeTns = new List<ChiTietBoDeTn>();

            if (model.SelectedQuestionIds != null)
            {
                int order = 1;
                foreach (var questionId in model.SelectedQuestionIds)
                {
                    boDe.ChiTietBoDeTns.Add(new ChiTietBoDeTn
                    {
                        IdBoDe = boDe.IdBoDe,
                        IdCauHoi = questionId,
                        ThuTu = order++
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật bộ đề thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================== DELETE (GET) ==================
        public async Task<IActionResult> Delete(int id)
        {
            var boDe = await _context.BoDeThiThus
                .Include(b => b.IdHangNavigation)
                .FirstOrDefaultAsync(b => b.IdBoDe == id);

            if (boDe == null)
                return NotFound();

            return View(boDe);
        }

        // ================== DELETE (POST) ==================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var boDe = await _context.BoDeThiThus
                .Include(b => b.ChiTietBoDeTns)
                .FirstOrDefaultAsync(b => b.IdBoDe == id);

            if (boDe == null)
                return NotFound();

            _context.ChiTietBoDeTns.RemoveRange(boDe.ChiTietBoDeTns);
            _context.BoDeThiThus.Remove(boDe);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int id)
        {
            var boDe = await _context.BoDeThiThus
                .Include(b => b.IdHangNavigation)
                .Include(b => b.ChiTietBoDeTns)
                    .ThenInclude(ct => ct.IdCauHoiNavigation)
                        .ThenInclude(c => c.DapAns)
                .FirstOrDefaultAsync(b => b.IdBoDe == id);

            if (boDe == null)
                return NotFound();

            return View(boDe);
        }
    }
}
