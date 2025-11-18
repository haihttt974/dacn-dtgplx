using dacn_dtgplx.Models;
using dacn_dtgplx.Services;
using dacn_dtgplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace dacn_dtgplx.Controllers
{
    public class AdminTheoryQuestionsController : Controller
    {
        private readonly DtGplxContext _context;
        private readonly SemanticSearchService _semantic;

        public AdminTheoryQuestionsController(DtGplxContext context, SemanticSearchService semantic)
        {
            _context = context;
            _semantic = semantic;
        }

        public async Task<IActionResult> Index(
    int? chuongId,
    bool? liet,
    bool? xemay,
    bool? chuy,
    string? keyword,
    int page = 1,
    bool partial = false)
        {
            int pageSize = 12;

            var query = _context.CauHoiLyThuyets
                .Include(x => x.Chuong)
                .AsQueryable();

            // FILTER
            if (chuongId.HasValue && chuongId.Value > 0)
                query = query.Where(x => x.ChuongId == chuongId.Value);

            if (liet.HasValue)
                query = query.Where(x => x.CauLiet == liet);

            if (xemay.HasValue)
                query = query.Where(x => x.XeMay == xemay);

            if (chuy.HasValue)
                query = query.Where(x => x.ChuY == chuy);

            // ====== SEMANTIC SEARCH ======
            List<int>? listIds = null;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                // GỌI PYTHON SEARCH
                listIds = await _semantic.SearchAsync(keyword);

                if (listIds == null || listIds.Count == 0)
                {
                    // Fallback: tìm theo Nội dung nếu semantic không ra kết quả
                    var kwLower = keyword.ToLower();
                    query = query.Where(x =>
                        x.NoiDung != null &&
                        x.NoiDung.ToLower().Contains(kwLower));
                }
                else
                {
                    query = query.Where(x => listIds.Contains(x.IdCauHoi));
                }
            }

            query = query
                .OrderBy(x => x.Chuong.ThuTu)
                .ThenBy(x => x.IdCauHoi);

            int total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            if (listIds != null && listIds.Count > 0 && !string.IsNullOrWhiteSpace(keyword))
            {
                items = items.OrderBy(x => listIds.IndexOf(x.IdCauHoi)).ToList();
            }

            var vm = new CauHoiIndexVM
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total,
                Chapters = await _context.Chuongs.OrderBy(x => x.ThuTu).ToListAsync(),
                Filter = new CauHoiFilter
                {
                    ChuongId = chuongId,
                    IsLiet = liet,
                    IsXeMay = xemay,
                    IsChuY = chuy,
                    Keyword = keyword
                }
            };

            // --------- BUILD HTML CÂU HỎI + PHÂN TRANG (CHO AJAX) ---------
            string BuildQuestionsHtml(CauHoiIndexVM model)
            {
                var colors = new[]
                {
                "#ff7675", "#74b9ff", "#55efc4",
                "#ffeaa7", "#a29bfe", "#fab1a0"
            };

                var sb = new StringBuilder();

                // LIST CARD
                sb.Append("<div class='row g-3'>");

                foreach (var q in model.Items)
                {
                    int colorIndex = (q.Chuong?.ThuTu ?? 1) - 1;
                    if (colorIndex < 0 || colorIndex >= colors.Length) colorIndex = 0;
                    string color = colors[colorIndex];

                    string img = string.IsNullOrWhiteSpace(q.HinhAnh)
                        ? ""
                        : q.HinhAnh.Replace("wwwroot", "");

                    string imgMeo = string.IsNullOrWhiteSpace(q.UrlAnhMeo)
                        ? ""
                        : q.UrlAnhMeo.Replace("wwwroot", "");

                    sb.Append("<div class='col-12'>");
                    sb.Append("<div class='question-card d-flex align-items-stretch'>");

                    sb.AppendFormat(
                        "<div class='indicator-bar' style='background:{0}'></div>",
                        color);

                    sb.Append("<div class='p-3 flex-grow-1 d-flex justify-content-between align-items-center'>");

                    // LEFT: text
                    sb.Append("<div>");
                    sb.AppendFormat("<div class='fw-semibold mb-1'>Câu hỏi số {0}</div>", q.IdCauHoi);
                    // BADGES trạng thái
                    sb.Append("<div class='d-flex gap-2 mt-1'>");

                    if (q.CauLiet == true)
                        sb.Append("<span class='badge bg-danger'>Câu liệt</span>");

                    if (q.ChuY == true)
                        sb.Append("<span class='badge bg-warning text-dark'>Chú ý</span>");

                    if (q.XeMay == true)
                        sb.Append("<span class='badge bg-primary'>Xe máy</span>");

                    sb.Append("</div>");

                    sb.Append("</div>");

                    // RIGHT: images + actions
                    sb.Append("<div class='d-flex align-items-center gap-3'>");

                    if (!string.IsNullOrEmpty(img))
                    {
                        sb.AppendFormat(
                            "<img src='{0}' class='thumb-img' onclick=\"showImage('{0}')\" />",
                            img);
                    }

                    sb.AppendFormat(
                        "<a class='btn btn-sm btn-warning text-white' href='/AdminTheoryQuestions/Answers/{0}'>Đáp án</a>",
                        q.IdCauHoi);

                    sb.Append("</div>"); // right
                    sb.Append("</div>"); // inner
                    sb.Append("</div>"); // card
                    sb.Append("</div>"); // col
                }

                sb.Append("</div>"); // row

                // PAGINATION MẪU
                int currentPage = model.Page;
                int totalPages = model.TotalPages;

                if (totalPages > 1)
                {
                    sb.Append("<nav class='mt-3'><ul class='pagination justify-content-center'>");

                    // Previous
                    sb.AppendFormat(
                        "<li class='page-item {0}'>",
                        currentPage == 1 ? "disabled" : "");
                    sb.AppendFormat(
                        "<a class='page-link page-btn' data-page='{0}'><i class='fa fa-chevron-left'></i></a></li>",
                        currentPage - 1);

                    int range = 2;
                    int start = Math.Max(1, currentPage - range);
                    int end = Math.Min(totalPages, currentPage + range);

                    // Page 1 + ...
                    if (start > 1)
                    {
                        sb.Append("<li class='page-item'><a class='page-link page-btn' data-page='1'>1</a></li>");
                        if (start > 2)
                        {
                            sb.Append("<li class='page-item disabled'><span class='page-link'>...</span></li>");
                        }
                    }

                    // Main pages
                    for (int i = start; i <= end; i++)
                    {
                        string activeClass = i == currentPage ? "active" : "";
                        sb.AppendFormat(
                            "<li class='page-item {0}'><a class='page-link page-btn' data-page='{1}'>{1}</a></li>",
                            activeClass, i);
                    }

                    // ... + Last
                    if (end < totalPages)
                    {
                        if (end < totalPages - 1)
                        {
                            sb.Append("<li class='page-item disabled'><span class='page-link'>...</span></li>");
                        }

                        sb.AppendFormat(
                            "<li class='page-item'><a class='page-link page-btn' data-page='{0}'>{0}</a></li>",
                            totalPages);
                    }

                    // Next
                    sb.AppendFormat(
                        "<li class='page-item {0}'>",
                        currentPage == totalPages ? "disabled" : "");
                    sb.AppendFormat(
                        "<a class='page-link page-btn' data-page='{0}'><i class='fa fa-chevron-right'></i></a></li>",
                        currentPage + 1);

                    sb.Append("</ul></nav>");
                }

                return sb.ToString();
            }

            string html = BuildQuestionsHtml(vm);

            // Nếu là gọi AJAX (partial=true) thì trả về HTML thô để JS chèn vào
            if (partial)
            {
                return Content(html, "text/html; charset=utf-8");
            }

            // Lần load đầu: render sẵn vào View
            ViewBag.InitialHtml = html;
            return View(vm);
        }

        public async Task<IActionResult> Answers(int id)
        {
            var q = await _context.CauHoiLyThuyets
                .Include(x => x.DapAns)
                .FirstOrDefaultAsync(x => x.IdCauHoi == id);

            if (q == null) return NotFound();

            return View(q);
        }
    }
}
