using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Controllers
{
    public class ThueXeController : Controller
    {
        private readonly DtGplxContext _context;
        private const int PageSize = 8;

        public ThueXeController(DtGplxContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.LoaiXeList = await _context.XeTapLais
                .Select(x => x.LoaiXe)
                .Distinct()
                .ToListAsync();

            var xeList = await _context.XeTapLais
                .Take(PageSize)
                .ToListAsync();

            int total = await _context.XeTapLais.CountAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)total / PageSize);

            return View(xeList);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string? search, decimal? min, decimal? max,
                                               string? type, string? sort,
                                               DateTime? rentDate, TimeSpan? rentTime,
                                               int page = 1)
        {
            var query = _context.XeTapLais.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.LoaiXe.Contains(search));

            // Filter Type
            if (!string.IsNullOrEmpty(type))
                query = query.Where(x => x.LoaiXe.Contains(type));

            // Price filter
            if (min.HasValue)
                query = query.Where(x => x.GiaThueTheoGio >= min);

            if (max.HasValue)
                query = query.Where(x => x.GiaThueTheoGio <= max);

            // Sort
            if (sort == "asc") query = query.OrderBy(x => x.GiaThueTheoGio);
            if (sort == "desc") query = query.OrderByDescending(x => x.GiaThueTheoGio);

            // Lọc xe theo ngày + giờ
            if (rentDate.HasValue && rentTime.HasValue)
            {
                var date = DateOnly.FromDateTime(rentDate.Value);
                var time = TimeOnly.FromTimeSpan(rentTime.Value);

                var rentStart = rentDate.Value.Date + rentTime.Value;
                var rentEnd = rentStart.AddHours(1); // mặc định thuê 1h

                // ---- LỊCH HỌC ----
                var busyFromLichHoc = await _context.LichHocs
                    .Where(l =>
                        l.KhoaHoc.IsActive == true &&
                        l.XeTapLaiId != null &&
                        l.NgayHoc == date &&
                        (
                            time >= l.TgBatDau.AddHours(-1) &&
                            time <= l.TgKetThuc.AddHours(1)
                        )
                    )
                    .Select(l => l.XeTapLaiId.Value)
                    .ToListAsync();

                // ---- XE ĐÃ ĐƯỢC THUÊ ----
                var busyFromThueXe = await _context.PhieuThueXe
                    .Where(p =>
                        p.TgBatDau.HasValue &&
                        p.TgThue.HasValue &&
                        p.HoaDonThanhToans.Any(h => h.TrangThai == true)
                    )
                    .Select(p => new {
                        p.XeId,
                        Start = p.TgBatDau.Value,
                        End = p.TgBatDau.Value.AddHours(p.TgThue.Value)
                    })
                    .ToListAsync();

                var busyThueXeIds = busyFromThueXe
                    .Where(b =>
                        rentStart < b.End.AddHours(1) &&
                        rentEnd > b.Start.AddHours(-1)
                    )
                    .Select(b => b.XeId)
                    .ToList();

                // ---- GỘP DANH SÁCH BẬN ----
                var allBusyXeIds = busyFromLichHoc
                    .Concat(busyThueXeIds)
                    .Distinct()
                    .ToList();

                // ---- LOẠI XE BẬN ----
                query = query.Where(x => !allBusyXeIds.Contains(x.XeTapLaiId));
            }

            // Pagination
            int total = await query.CountAsync();
            int totalPage = (int)Math.Ceiling((double)total / PageSize);

            query = query.Skip((page - 1) * PageSize).Take(PageSize);

            var list = await query.ToListAsync();

            ViewBag.TotalPages = totalPage;
            ViewBag.CurrentPage = page;

            return PartialView("_XeCards", list);
        }
    }
}
