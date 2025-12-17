using dacn_dtgplx.DTOs;
using dacn_dtgplx.Models.Requests;
using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    [Route("AdminReport")]
    public class AdminReportController : Controller
    {
        private readonly ReportService _reportService;

        public AdminReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // ======================================================
        // ===================== MAIN VIEW ======================
        // ======================================================

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var model = await _reportService.GetDashboardAsync(fromDate, toDate);
            return View("~/Views/AdminReport/Index.cshtml", model);
        }

        // ======================================================
        // ===================== TAB LOAD (AJAX) =================
        // ======================================================

        [HttpGet("Tab/{tab}")]
        public async Task<IActionResult> Tab(
            string tab,
            DateTime? fromDate,
            DateTime? toDate)
        {
            tab = (tab ?? "overview").ToLower();

            var model = await _reportService.GetDashboardAsync(fromDate, toDate);

            return tab switch
            {
                "users" => PartialView("~/Views/AdminReport/_TabUsers.cshtml", model),
                "courses" => PartialView("~/Views/AdminReport/_TabCourses.cshtml", model),
                "tests" => PartialView("~/Views/AdminReport/_TabTests.cshtml", model),
                _ => PartialView("~/Views/AdminReport/_TabOverview.cshtml", model),
            };
        }

        // ======================================================
        // ===================== EXPORT PDF =====================
        // ======================================================

        [HttpPost("ExportPdf")]
        public async Task<IActionResult> ExportPdf([FromBody] ExportPdfRequest req)
        {
            if (req == null)
                return BadRequest("Request body is null");

            var tab = (req.Tab ?? "overview").ToLower();

            // 1️⃣ Lấy lại dữ liệu dashboard theo bộ lọc
            var model = await _reportService.GetDashboardAsync(
                req.FromDate,
                req.ToDate
            );

            // 2️⃣ Gọi Service export PDF (ĐÚNG CHỮ KÝ MỚI)
            var pdfBytes = _reportService.GenerateDashboardPdf(
                model,
                req.FromDate,
                req.ToDate,
                tab,
                req.Charts ?? new List<ChartImageDto>(),   // ⚠ BIỂU ĐỒ TỪ VIEW (BASE64)
                req.ConfirmText ?? "Báo cáo được xuất bởi hệ thống",
                req.WatermarkText ?? "CONFIDENTIAL"
            );

            // 3️⃣ Trả file PDF
            var fileName = $"BaoCao_{tab}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
