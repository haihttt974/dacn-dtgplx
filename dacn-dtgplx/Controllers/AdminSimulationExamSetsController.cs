using dacn_dtgplx.Models;
using dacn_dtgplx.Models.Requests;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dacn_dtgplx.Controllers
{
    public class AdminSimulationExamSetsController : Controller
    {
        private readonly DtGplxContext _context;

        public AdminSimulationExamSetsController(DtGplxContext context)
        {
            _context = context;
        }

        // ===================== DANH SÁCH BỘ ĐỀ =====================
        public async Task<IActionResult> Index(string? status, int? hard)
        {
            var query = _context.BoDeMoPhongs
                .Include(b => b.ChiTietBoDeMoPhongs)
                .ThenInclude(c => c.IdThMpNavigation)
                .AsQueryable();

            //// Lọc theo trạng thái
            //if (!string.IsNullOrEmpty(status))
            //{
            //    if (status == "active")
            //        query = query.Where(x => x.IsActive == true);
            //    else if (status == "inactive")
            //        query = query.Where(x => x.IsActive == false);
            //}

            //// Lọc theo số câu khó
            //if (hard.HasValue)
            //{
            //    query = query.Where(b =>
            //        b.ChiTietBoDeMoPhongs
            //         .Count(ct => ct.IdThMpNavigation.Kho == true) == hard.Value
            //    );
            //}

            var list = await query
                .OrderByDescending(b => b.TaoLuc)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string? status, int? hard)
        {
            var query = _context.BoDeMoPhongs
                .Include(b => b.ChiTietBoDeMoPhongs)
                .ThenInclude(ct => ct.IdThMpNavigation)
                .AsQueryable();

            // Lọc trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                query = query.Where(x => x.IsActive == isActive);
            }

            // Lọc theo số câu khó
            if (hard.HasValue)
            {
                query = query.Where(x =>
                    x.ChiTietBoDeMoPhongs
                     .Count(ct => ct.IdThMpNavigation.Kho == true) == hard.Value
                );
            }

            var list = await query
                .OrderByDescending(x => x.TaoLuc)
                .ToListAsync();

            // Trả lại đúng PartialView vừa tách
            return PartialView("_ExamSetTable", list);
        }

        // ===================== TẠO BỘ ĐỀ =====================
        [HttpPost]
        public async Task<IActionResult> CreateExamSet(string TenBoDe, int SoCauKho)
        {
            // ========== VALIDATION ==========
            if (SoCauKho <= 0 || SoCauKho > 10)
            {
                TempData["Error"] = "Số câu khó phải từ 1 đến 10!";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(TenBoDe))
            {
                int total = await _context.BoDeMoPhongs.CountAsync();
                TenBoDe = $"Bộ đề số {total + 1}";
            }

            // ========== TỶ LỆ CHƯƠNG ==========
            var tiLe = new Dictionary<int, int>()
            {
                { 1, 2 }, // 20%
                { 2, 1 }, // 10%
                { 3, 2 },
                { 4, 1 },
                { 5, 2 },
                { 6, 2 }
            };

            var rnd = new Random();
            var selectedIds = new List<int>();

            // ========== RANDOM THEO TỶ LỆ ==========
            foreach (var item in tiLe)
            {
                int idChuong = item.Key;
                int soCau = item.Value;

                var cauTrongChuong = await _context.TinhHuongMoPhongs
                    .Where(x => x.IdChuongMp == idChuong)
                    .ToListAsync();

                if (cauTrongChuong.Count < soCau)
                {
                    TempData["Error"] = $"Không đủ câu ở chương {idChuong}";
                    return RedirectToAction("Index");
                }

                var chon = cauTrongChuong
                    .OrderBy(_ => rnd.Next())
                    .Take(soCau)
                    .ToList();

                selectedIds.AddRange(chon.Select(x => x.IdThMp));
            }

            // Lấy danh sách chi tiết để phân tích
            var selectedTH = await _context.TinhHuongMoPhongs
                .Where(x => selectedIds.Contains(x.IdThMp))
                .ToListAsync();

            int currentlyHard = selectedTH.Count(x => x.Kho == true);

            // ========== CASE 1: THIẾU CÂU KHÓ ==========
            if (currentlyHard < SoCauKho)
            {
                int need = SoCauKho - currentlyHard;

                var hardPool = await _context.TinhHuongMoPhongs
                    .Where(x => x.Kho == true && !selectedIds.Contains(x.IdThMp))
                    .ToListAsync();

                if (hardPool.Count < need)
                {
                    TempData["Error"] = "Không đủ câu khó trong ngân hàng!";
                    return RedirectToAction("Index");
                }

                var extraHard = hardPool
                    .OrderBy(_ => rnd.Next())
                    .Take(need)
                    .ToList();

                // Loại câu dễ để thay thế
                var removeEasy = selectedTH
                    .Where(x => x.Kho == false)
                    .Take(need)
                    .ToList();

                foreach (var th in removeEasy)
                    selectedIds.Remove(th.IdThMp);

                selectedIds.AddRange(extraHard.Select(x => x.IdThMp));
            }

            // ========== CASE 2: THỪA CÂU KHÓ ==========
            if (currentlyHard > SoCauKho)
            {
                int needRemove = currentlyHard - SoCauKho;

                // Câu khó bị loại bớt
                var hardToRemove = selectedTH
                    .Where(x => x.Kho == true)
                    .Take(needRemove)
                    .ToList();

                // Pool câu dễ để thay
                var easyPool = await _context.TinhHuongMoPhongs
                    .Where(x => (x.Kho == false || x.Kho == null)
                                && !selectedIds.Contains(x.IdThMp))
                    .ToListAsync();

                if (easyPool.Count < needRemove)
                {
                    TempData["Error"] = "Không đủ câu dễ để thay thế!";
                    return RedirectToAction("Index");
                }

                // Xóa câu khó dư
                foreach (var th in hardToRemove)
                    selectedIds.Remove(th.IdThMp);

                // Thêm câu dễ
                var extraEasy = easyPool
                    .OrderBy(_ => rnd.Next())
                    .Take(needRemove)
                    .ToList();

                selectedIds.AddRange(extraEasy.Select(x => x.IdThMp));
            }

            // ========== TẠO BỘ ĐỀ ==========
            var newExam = new BoDeMoPhong
            {
                TenBoDe = TenBoDe,
                TaoLuc = DateTime.Now,
                IsActive = true,
                SoTinhHuong = 10
            };

            _context.BoDeMoPhongs.Add(newExam);
            await _context.SaveChangesAsync();

            // ========== LƯU CHI TIẾT ==========
            int order = 1;
            foreach (var idTH in selectedIds)
            {
                _context.ChiTietBoDeMoPhongs.Add(new ChiTietBoDeMoPhong
                {
                    IdBoDeMoPhong = newExam.IdBoDeMoPhong,
                    IdThMp = idTH,
                    ThuTu = order++
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tạo bộ đề '{TenBoDe}' thành công!";
            return RedirectToAction("Index");
        }

        // ===================== MÀN HÌNH QUẢN LÝ =====================
        public async Task<IActionResult> Manage(int id)
        {
            var boDe = await _context.BoDeMoPhongs
                .Include(c => c.ChiTietBoDeMoPhongs)
                .FirstOrDefaultAsync(c => c.IdBoDeMoPhong == id);

            if (boDe == null) return NotFound();

            // Lấy 10 tình huống
            var selected = await _context.ChiTietBoDeMoPhongs
                .Where(c => c.IdBoDeMoPhong == id)
                .OrderBy(c => c.ThuTu)
                .Include(c => c.IdThMpNavigation)
                .ToListAsync();

            var selectedVM = selected.Select(x => new SelectedSituationVM
            {
                IdThMp = x.IdThMp,
                TieuDe = x.IdThMpNavigation.TieuDe,
                VideoUrl = x.IdThMpNavigation.VideoUrl,
                UrlAnhMeo = x.IdThMpNavigation.UrlAnhMeo,
                Kho = x.IdThMpNavigation.Kho ?? false,
                ThuTu = x.ThuTu
            }).ToList();

            var vm = new SimulationExamSetManageViewModel
            {
                IdBoDe = id,
                TenBoDe = boDe.TenBoDe,
                SelectedDetails = selectedVM,
                AllChapters = await _context.ChuongMoPhongs.OrderBy(x => x.ThuTu).ToListAsync(),
                AllSituations = await _context.TinhHuongMoPhongs.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Manage(int id, List<int> SelectedIds)
        {
            var exam = await _context.BoDeMoPhongs
                .Include(x => x.ChiTietBoDeMoPhongs)
                .FirstOrDefaultAsync(x => x.IdBoDeMoPhong == id);

            if (exam == null)
                return NotFound();

            _context.ChiTietBoDeMoPhongs.RemoveRange(exam.ChiTietBoDeMoPhongs);

            int order = 1;
            foreach (var thId in SelectedIds)
            {
                _context.ChiTietBoDeMoPhongs.Add(new ChiTietBoDeMoPhong
                {
                    IdBoDeMoPhong = id,
                    IdThMp = thId,
                    ThuTu = order++
                });
            }

            exam.SoTinhHuong = SelectedIds.Count;
            if (exam.SoTinhHuong < 10) exam.IsActive = false;
            else exam.IsActive = true;
            
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Lưu bộ đề '{exam.TenBoDe}' thành công!";

            return RedirectToAction("Index");
        }

        // ===================== RANDOM TÌNH HUỐNG (AJAX) =====================
        [HttpGet]
        public async Task<IActionResult> Random(int chapter, int level, string? used)
        {
            var usedIds = new List<int>();

            if (!string.IsNullOrEmpty(used))
                usedIds = used.Split(',').Select(int.Parse).ToList();

            var query = _context.TinhHuongMoPhongs.AsQueryable();

            // Filter chương
            if (chapter > 0)
                query = query.Where(x => x.IdChuongMp == chapter);

            // Filter độ khó
            if (level == 1)
                query = query.Where(x => x.Kho == true);
            else if (level == 0)
                query = query.Where(x => x.Kho == false || x.Kho == null);

            // Không lấy TH đang dùng
            if (usedIds.Any())
                query = query.Where(x => !usedIds.Contains(x.IdThMp));

            var list = await query.ToListAsync();

            if (!list.Any())
            {
                return Json(new { ok = false, message = "Không tìm thấy tình huống phù hợp!" });
            }

            var rnd = new Random();
            var chosen = list[rnd.Next(list.Count)];

            return Json(new
            {
                ok = true,
                id = chosen.IdThMp,
                tieuDe = chosen.TieuDe,
                video = string.IsNullOrEmpty(chosen.VideoUrl)
                        ? null
                        : "/" + chosen.VideoUrl.Replace("wwwroot/", ""),
                anhMeo = string.IsNullOrEmpty(chosen.UrlAnhMeo)
                        ? null
                        : "/" + chosen.UrlAnhMeo.Replace("wwwroot/", ""),
                kho = chosen.Kho ?? false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusRow([FromBody] ToggleStatusRequest req)
        {
            int id = req.id;

            var exam = await _context.BoDeMoPhongs
                .Include(x => x.ChiTietBoDeMoPhongs)
                .FirstOrDefaultAsync(x => x.IdBoDeMoPhong == id);

            if (exam == null)
                return Json(new { ok = false, message = "Không tìm thấy bộ đề!" });

            if (exam.IsActive != true && exam.ChiTietBoDeMoPhongs.Count < 10)
            {
                return Json(new
                {
                    ok = false,
                    message = $"Bộ đề '{exam.TenBoDe}' chưa đủ 10 tình huống!"
                });
            }

            exam.IsActive = !exam.IsActive;
            await _context.SaveChangesAsync();

            string html = await this.RenderViewAsync("_ExamSetRow", exam, true);

            return Json(new
            {
                ok = true,
                html,
                toast = (bool)exam.IsActive ? "Đã mở bộ đề!" : "Đã đóng bộ đề!"
            });
        }

    }
}