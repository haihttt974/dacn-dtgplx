using dacn_dtgplx.DTOs;
using dacn_dtgplx.Models;
using dacn_dtgplx.ViewModels.Report;
using dacn_dtgplx.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace dacn_dtgplx.Services
{
    public class ReportService
    {
        private readonly DtGplxContext _context;

        // ✅ Logo theo yêu cầu
        private readonly string _logoPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/images/Logo/logo.jpg"
        );

        public ReportService(DtGplxContext context)
        {
            _context = context;
        }

        // ======================================================
        // ===================== DASHBOARD DATA =================
        // ======================================================
        public async Task<DashboardReportVM> GetDashboardAsync(DateTime? fromDate, DateTime? toDate)
        {
            var fromDO = ToDateOnly(fromDate);
            var toDO = ToDateOnly(toDate);

            var model = new DashboardReportVM
            {
                Filter = new ReportFilterVM
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    OnlySuccessfulPayments = true
                },
                RangeText = FormatRange(fromDate, toDate),
            };

            await BuildOverview(model, fromDate, toDate);
            await BuildUsers(model, fromDO, toDO);
            await BuildCourses(model, fromDate, toDate, fromDO, toDO);
            await BuildTests(model);

            // KPI Cards (hiển thị chung phía trên)
            model.TopKpis = new List<KpiCardVM>
            {
                new KpiCardVM{ Title="Tổng doanh thu", ValueText=$"{model.Overview.TongDoanhThu:N0}", Unit="VND", SubTitle="Theo bộ lọc", IconCss="fas fa-coins" },
                new KpiCardVM{ Title="Giao dịch", ValueText=$"{model.Overview.SoGiaoDich:N0}", Unit="GD", SubTitle="Theo bộ lọc", IconCss="fas fa-receipt" },
                new KpiCardVM{ Title="Người mới", ValueText=$"{model.Users.SoNguoiMoi:N0}", Unit="HV", SubTitle="Theo hồ sơ đăng ký", IconCss="fas fa-user-plus" },
                new KpiCardVM{ Title="Khóa học mới", ValueText=$"{model.Courses.SoKhoaHocMoi:N0}", Unit="KH", SubTitle="Theo bộ lọc", IconCss="fas fa-book" },
            };

            return model;
        }

        // ======================================================
        // ===================== OVERVIEW =======================
        // ======================================================
        private async Task BuildOverview(DashboardReportVM model, DateTime? fromDate, DateTime? toDate)
        {
            var payQuery = _context.HoaDonThanhToans.AsNoTracking().AsQueryable()
                .Where(x => x.NgayThanhToan.HasValue)
                .Where(x => x.SoTien.HasValue);

            if (fromDate.HasValue) payQuery = payQuery.Where(x => x.NgayThanhToan!.Value >= fromDate.Value);
            if (toDate.HasValue) payQuery = payQuery.Where(x => x.NgayThanhToan!.Value <= toDate.Value);

            if (model.Filter.OnlySuccessfulPayments == true)
                payQuery = payQuery.Where(x => x.TrangThai == true);

            var allPayments = await payQuery.ToListAsync();

            var coursePayments = allPayments.Where(x => x.IdDangKy.HasValue).ToList();
            var vehiclePayments = allPayments.Where(x => x.PhieuTxId.HasValue).ToList();

            decimal sumAll = allPayments.Sum(x => x.SoTien ?? 0m);
            decimal sumCourse = coursePayments.Sum(x => x.SoTien ?? 0m);
            decimal sumVehicle = vehiclePayments.Sum(x => x.SoTien ?? 0m);

            // revenue by month multi-series
            var courseMonth = coursePayments
                .GroupBy(x => new { Y = x.NgayThanhToan!.Value.Year, M = x.NgayThanhToan!.Value.Month })
                .Select(g => new { g.Key.Y, g.Key.M, Sum = g.Sum(x => x.SoTien ?? 0m) })
                .ToDictionary(x => $"{x.M}/{x.Y}", x => x.Sum);

            var vehicleMonth = vehiclePayments
                .GroupBy(x => new { Y = x.NgayThanhToan!.Value.Year, M = x.NgayThanhToan!.Value.Month })
                .Select(g => new { g.Key.Y, g.Key.M, Sum = g.Sum(x => x.SoTien ?? 0m) })
                .ToDictionary(x => $"{x.M}/{x.Y}", x => x.Sum);

            var monthLabels = courseMonth.Keys
                .Union(vehicleMonth.Keys)
                .Select(s => new { Label = s, Key = ParseMonthKey(s) })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                .Select(x => x.Label)
                .ToList();

            model.Overview.TongDoanhThu = sumAll;
            model.Overview.DoanhThuKhoaHoc = sumCourse;
            model.Overview.DoanhThuThueXe = sumVehicle;

            model.Overview.SoGiaoDich = allPayments.Count;
            model.Overview.SoGiaoDichKhoaHoc = coursePayments.Count;
            model.Overview.SoGiaoDichThueXe = vehiclePayments.Count;

            var distinctDays = allPayments.Select(x => x.NgayThanhToan!.Value.Date).Distinct().Count();
            model.Overview.DoanhThuTrungBinhNgay = distinctDays > 0 ? sumAll / distinctDays : 0m;

            var distinctMonths = monthLabels.Count;
            model.Overview.DoanhThuTrungBinhThang = distinctMonths > 0 ? sumAll / distinctMonths : 0m;

            model.Overview.RevenueByMonth_Multi.Labels = monthLabels;
            model.Overview.RevenueByMonth_Multi.Series = new List<SeriesVM>
            {
                new SeriesVM
                {
                    Name = "Doanh thu khóa học",
                    Data = monthLabels.Select(lb => courseMonth.TryGetValue(lb, out var v) ? v : 0m).ToList()
                },
                new SeriesVM
                {
                    Name = "Doanh thu thuê xe",
                    Data = monthLabels.Select(lb => vehicleMonth.TryGetValue(lb, out var v) ? v : 0m).ToList()
                }
            };

            model.Overview.RevenueSharePie = new List<PieSliceVM>
            {
                new PieSliceVM{ Label="Khóa học", Value=(int)sumCourse },
                new PieSliceVM{ Label="Thuê xe", Value=(int)sumVehicle }
            };

            model.Overview.RevenueByMonthTable = monthLabels.Select(lb =>
            {
                var c = courseMonth.TryGetValue(lb, out var cv) ? cv : 0m;
                var v = vehicleMonth.TryGetValue(lb, out var vv) ? vv : 0m;

                return new RevenueMonthRowVM
                {
                    MonthLabel = lb,
                    RevenueCourses = c,
                    RevenueVehicles = v,
                    RevenueTotal = c + v,
                    Transactions = allPayments.Count(p => $"{p.NgayThanhToan!.Value.Month}/{p.NgayThanhToan!.Value.Year}" == lb)
                };
            }).ToList();

            // Top khóa học theo doanh thu
            var coursePaymentEnrollIds = coursePayments.Select(x => x.IdDangKy!.Value).Distinct().ToList();
            var enrolls = await _context.DangKyHocs.AsNoTracking()
                .Include(x => x.KhoaHoc)
                .Where(x => coursePaymentEnrollIds.Contains(x.IdDangKy))
                .ToListAsync();

            var courseNameByEnrollId = enrolls.ToDictionary(
                x => x.IdDangKy,
                x => x.KhoaHoc?.TenKhoaHoc ?? $"KH#{x.KhoaHocId}"
            );

            model.Overview.TopCoursesByRevenue = coursePayments
                .GroupBy(p =>
                {
                    var id = p.IdDangKy!.Value;
                    return courseNameByEnrollId.TryGetValue(id, out var name) ? name : $"ĐK#{id}";
                })
                .Select(g => new PieSliceVM { Label = g.Key, Value = (int)g.Sum(x => x.SoTien ?? 0m) })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            // Top xe theo doanh thu
            var phieuIds = vehiclePayments.Select(x => x.PhieuTxId!.Value).Distinct().ToList();
            var phieuThue = await _context.PhieuThueXe.AsNoTracking()
                .Include(p => p.Xe)
                .Where(p => phieuIds.Contains(p.PhieuTxId))
                .ToListAsync();

            var xeLabelByPhieuId = phieuThue.ToDictionary(
                x => x.PhieuTxId,
                x =>
                {
                    var bienSo = x.Xe?.BienSo;
                    var loai = x.Xe?.LoaiXe ?? $"Xe#{x.XeId}";
                    return !string.IsNullOrWhiteSpace(bienSo) ? $"{loai} ({bienSo})" : loai;
                });

            model.Overview.TopVehiclesByRevenue = vehiclePayments
                .GroupBy(p =>
                {
                    var id = p.PhieuTxId!.Value;
                    return xeLabelByPhieuId.TryGetValue(id, out var name) ? name : $"Phiếu#{id}";
                })
                .Select(g => new PieSliceVM { Label = g.Key, Value = (int)g.Sum(x => x.SoTien ?? 0m) })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();
        }

        // ======================================================
        // ===================== USERS ==========================
        // ======================================================
        private async Task BuildUsers(DashboardReportVM model, DateOnly? fromDO, DateOnly? toDO)
        {
            model.Users.TongNguoiDung = await _context.Users.AsNoTracking().CountAsync();

            var hoSoQuery = _context.HoSoThiSinhs.AsNoTracking().AsQueryable()
                .Where(x => x.NgayDk.HasValue);

            if (fromDO.HasValue) hoSoQuery = hoSoQuery.Where(x => x.NgayDk!.Value >= fromDO.Value);
            if (toDO.HasValue) hoSoQuery = hoSoQuery.Where(x => x.NgayDk!.Value <= toDO.Value);

            model.Users.SoHoSoMoi = await hoSoQuery.CountAsync();
            model.Users.SoNguoiMoi = await hoSoQuery.Select(x => x.UserId).Distinct().CountAsync();

            var hoSoRaw = await hoSoQuery
                .GroupBy(x => new { Y = x.NgayDk!.Value.Year, M = x.NgayDk!.Value.Month })
                .Select(g => new
                {
                    g.Key.Y,
                    g.Key.M,
                    NewProfiles = g.Count(),
                    NewUsers = g.Select(x => x.UserId).Distinct().Count()
                })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            model.Users.NewUsersByMonth = hoSoRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.NewUsers })
                .ToList();

            model.Users.NewProfilesByMonth = hoSoRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.NewProfiles })
                .ToList();

            model.Users.UsersByMonthTable = hoSoRaw
                .Select(x => new UserMonthRowVM
                {
                    MonthLabel = $"{x.M}/{x.Y}",
                    NewUsers = x.NewUsers,
                    NewProfiles = x.NewProfiles
                })
                .ToList();
        }

        // ======================================================
        // ===================== COURSES ========================
        // ======================================================
        private async Task BuildCourses(
            DashboardReportVM model,
            DateTime? fromDate,
            DateTime? toDate,
            DateOnly? fromDO,
            DateOnly? toDO)
        {
            var courseQuery = _context.KhoaHocs.AsNoTracking().AsQueryable()
                .Where(x => x.NgayBatDau.HasValue);

            if (fromDate.HasValue) courseQuery = courseQuery.Where(x => x.NgayBatDau!.Value >= fromDate.Value);
            if (toDate.HasValue) courseQuery = courseQuery.Where(x => x.NgayBatDau!.Value <= toDate.Value);

            model.Courses.SoKhoaHocMoi = await courseQuery.CountAsync();

            var courseRaw = await courseQuery
                .GroupBy(x => new { Y = x.NgayBatDau!.Value.Year, M = x.NgayBatDau!.Value.Month })
                .Select(g => new { g.Key.Y, g.Key.M, Count = g.Count() })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            model.Courses.NewCoursesByMonth = courseRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.Count })
                .ToList();

            // enrollments
            var enrollQuery = _context.DangKyHocs.AsNoTracking().AsQueryable()
                .Where(x => x.NgayDangKy.HasValue);

            if (fromDO.HasValue) enrollQuery = enrollQuery.Where(x => x.NgayDangKy!.Value >= fromDO.Value);
            if (toDO.HasValue) enrollQuery = enrollQuery.Where(x => x.NgayDangKy!.Value <= toDO.Value);

            model.Courses.SoDangKyMoi = await enrollQuery.CountAsync();
            model.Courses.TongDangKy = await _context.DangKyHocs.AsNoTracking().CountAsync();

            var enrollRaw = await enrollQuery
                .GroupBy(x => new { Y = x.NgayDangKy!.Value.Year, M = x.NgayDangKy!.Value.Month })
                .Select(g => new { g.Key.Y, g.Key.M, Count = g.Count() })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            model.Courses.NewEnrollmentsByMonth = enrollRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.Count })
                .ToList();

            // table merge (course + enroll)
            var courseMonths = model.Courses.NewCoursesByMonth.Select(x => x.Label).ToHashSet();
            var enrollMonths = model.Courses.NewEnrollmentsByMonth.Select(x => x.Label).ToHashSet();
            var allMonthsCourses = courseMonths.Union(enrollMonths)
                .Select(s => new { Label = s, Key = ParseMonthKey(s) })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                .Select(x => x.Label)
                .ToList();

            model.Courses.CoursesByMonthTable = allMonthsCourses.Select(lb =>
            {
                var newC = model.Courses.NewCoursesByMonth.FirstOrDefault(x => x.Label == lb)?.Value ?? 0m;
                var newE = model.Courses.NewEnrollmentsByMonth.FirstOrDefault(x => x.Label == lb)?.Value ?? 0m;
                return new CourseMonthRowVM
                {
                    MonthLabel = lb,
                    NewCourses = (int)newC,
                    NewEnrollments = (int)newE
                };
            }).ToList();

            // Courses by Hang pie
            var coursesWithHang = await courseQuery
                .Include(x => x.IdHangNavigation)
                .ToListAsync();

            model.Courses.CoursesByHangPie = coursesWithHang
                .Where(x => x.IdHangNavigation != null)
                .GroupBy(x => x.IdHangNavigation!.MaHang ?? x.IdHang.ToString())
                .Select(g => new PieSliceVM { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            // Top courses by enrollments
            var enrollWithCourse = await enrollQuery
                .Include(x => x.KhoaHoc)
                .ToListAsync();

            model.Courses.TopCoursesByEnrollments = enrollWithCourse
                .GroupBy(x => x.KhoaHoc?.TenKhoaHoc ?? $"KH#{x.KhoaHocId}")
                .Select(g => new PieSliceVM { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();
        }

        // ======================================================
        // ===================== TESTS ==========================
        // ======================================================
        private async Task BuildTests(DashboardReportVM model)
        {
            var baiLamQuery = _context.BaiLams.AsNoTracking().AsQueryable();
            var baiMpQuery = _context.BaiLamMoPhongs.AsNoTracking().AsQueryable();

            model.Tests.TongLuotThiLyThuyet = await baiLamQuery.CountAsync();
            model.Tests.TongLuotThiMoPhong = await baiMpQuery.CountAsync();

            model.Tests.HocVienThiLyThuyet = await baiLamQuery.Select(x => x.UserId).Distinct().CountAsync();
            model.Tests.HocVienThiMoPhong = await baiMpQuery.Select(x => x.UserId).Distinct().CountAsync();

            // Pie: Theory by Hang
            var baiLamWithHang = await baiLamQuery
                .Include(x => x.IdBoDeNavigation)
                .ThenInclude(b => b.IdHangNavigation)
                .ToListAsync();

            model.Tests.TheoryByHangPie = baiLamWithHang
                .GroupBy(x => x.IdBoDeNavigation?.IdHangNavigation?.MaHang ?? $"H#{x.IdBoDeNavigation?.IdHang}")
                .Select(g => new PieSliceVM { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            // Pie: Simulation by BoDeMoPhong
            var baiMpWithBoDe = await baiMpQuery
                .Include(x => x.IdBoDeMoPhongNavigation)
                .ToListAsync();

            model.Tests.SimulationByBoDePie = baiMpWithBoDe
                .GroupBy(x => x.IdBoDeMoPhongNavigation?.TenBoDe ?? $"MP#{x.IdBoDeMoPhong}")
                .Select(g => new PieSliceVM { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            // ⚠ nếu chưa có cột thời gian làm bài thì để trống chart theo tháng
            model.Tests.AttemptsByMonth_Multi.Labels = new();
            model.Tests.AttemptsByMonth_Multi.Series = new();
            model.Tests.UsersByMonth_Multi.Labels = new();
            model.Tests.UsersByMonth_Multi.Series = new();
            model.Tests.TestsByMonthTable = new();
        }

        // ======================================================
        // ===================== PDF EXPORT =====================
        // ======================================================
        public byte[] GenerateDashboardPdf(
            DashboardReportVM model,
            DateTime? fromDate,
            DateTime? toDate,
            string tab,
            List<ChartImageDto> charts,
            string confirmText,
            string watermarkText
        )
        {
            var rangeText = FormatRange(fromDate, toDate);
            var title = TabTitle(tab);

            var hasLogo = File.Exists(_logoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ===================== WATERMARK =====================
                    if (!string.IsNullOrWhiteSpace(watermarkText))
                    {
                        page.Background()
                            .PaddingHorizontal(120)
                            .PaddingVertical(180)
                            .AlignCenter()
                            .AlignMiddle()
                            .Rotate(-45)
                            .Text(watermarkText.Trim())
                            .FontSize(52)
                            .Bold()
                            .FontColor(Colors.Grey.Darken2.WithAlpha(35));
                    }

                    //// ✅ Watermark ngang dưới (Footer)
                    //page.Footer()
                    //    .AlignCenter()
                    //    .PaddingBottom(20)
                    //    .Text(t =>
                    //    {
                    //        t.Span(watermarkText ?? "")
                    //         .FontSize(14)
                    //         .Italic()
                    //         .FontColor(Colors.Grey.Darken2.WithAlpha(120));
                    //    });

                    // ===================== HEADER =====================
                    page.Header().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(title).FontSize(18).Bold().FontColor(Colors.Black);
                            col.Item().PaddingTop(2).Text($"Thời gian: {rangeText}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(80).AlignRight().AlignMiddle().Element(e =>
                        {
                            if (hasLogo)
                                e.Image(_logoPath).FitArea();
                            else
                                e.Border(1).BorderColor(Colors.Grey.Lighten2)
                                 .AlignCenter().AlignMiddle()
                                 .Text("LOGO").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // ===================== CONTENT =====================
                    page.Content().Column(col =>
                    {
                        col.Spacing(14);

                        col.Item().Text("I. TỔNG QUAN CHỈ SỐ").Bold().FontSize(14);
                        col.Item().Element(c => BuildSummaryCards(c, model, tab));

                        col.Item().LineHorizontal(1);

                        col.Item().Text("II. BIỂU ĐỒ PHÂN TÍCH").Bold().FontSize(14);
                        BuildChartsFromView(col, charts);

                        col.Item().LineHorizontal(1);

                        col.Item().Text("III. BẢNG DỮ LIỆU CHI TIẾT").Bold().FontSize(14);

                        switch ((tab ?? "overview").ToLower())
                        {
                            case "users":
                                BuildUsersTables(col, model);
                                break;
                            case "courses":
                                BuildCoursesTables(col, model);
                                break;
                            case "tests":
                                BuildTestsTables(col, model);
                                break;
                            default:
                                BuildOverviewTables(col, model);
                                break;
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Spacing(4);

                        // ===== Watermark ngang (CONFIDENTIAL - HAILOC) =====
                        if (!string.IsNullOrWhiteSpace(watermarkText))
                        {
                            col.Item()
                                .AlignCenter()
                                .Text(watermarkText)
                                .FontSize(12)
                                .Italic()
                                .FontColor(Colors.Grey.Darken2.WithAlpha(120));
                        }

                        // ===== Divider =====
                        col.Item().LineHorizontal(1);

                        // ===== Confirm text =====
                        col.Item()
                            .AlignCenter()
                            .Text(confirmText ?? "Báo cáo được xuất bởi hệ thống")
                            .Italic()
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);

                        // ===== Timestamp =====
                        col.Item()
                            .AlignCenter()
                            .Text($"Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ======================================================
        // ===================== PDF: SUMMARY ===================
        // ======================================================
        private void BuildSummaryCards(IContainer container, DashboardReportVM model, string tab)
        {
            tab = (tab ?? "overview").ToLower();

            container.Row(row =>
            {
                row.Spacing(10);

                // Overview
                if (tab == "overview")
                {
                    row.RelativeItem().Element(e => Card(e, "Tổng doanh thu", $"{model.Overview.TongDoanhThu:N0} VND"));
                    row.RelativeItem().Element(e => Card(e, "Số giao dịch", $"{model.Overview.SoGiaoDich:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Người mới", $"{model.Users.SoNguoiMoi:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Doanh thu TB/tháng", $"{model.Overview.DoanhThuTrungBinhThang:N0}"));
                    return;
                }

                if (tab == "users")
                {
                    row.RelativeItem().Element(e => Card(e, "Người mới", $"{model.Users.SoNguoiMoi:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Hồ sơ mới", $"{model.Users.SoHoSoMoi:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Tổng người dùng", $"{model.Users.TongNguoiDung:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Khoảng thời gian", model.RangeText));
                    return;
                }

                if (tab == "courses")
                {
                    row.RelativeItem().Element(e => Card(e, "Khóa học mới", $"{model.Courses.SoKhoaHocMoi:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Đăng ký mới", $"{model.Courses.SoDangKyMoi:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Tổng đăng ký", $"{model.Courses.TongDangKy:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Khoảng thời gian", model.RangeText));
                    return;
                }

                if (tab == "tests")
                {
                    row.RelativeItem().Element(e => Card(e, "Lượt thi LT", $"{model.Tests.TongLuotThiLyThuyet:N0}"));
                    row.RelativeItem().Element(e => Card(e, "HV thi LT", $"{model.Tests.HocVienThiLyThuyet:N0}"));
                    row.RelativeItem().Element(e => Card(e, "Lượt thi MP", $"{model.Tests.TongLuotThiMoPhong:N0}"));
                    row.RelativeItem().Element(e => Card(e, "HV thi MP", $"{model.Tests.HocVienThiMoPhong:N0}"));
                    return;
                }

                // fallback
                row.RelativeItem().Element(e => Card(e, "Tổng doanh thu", $"{model.Overview.TongDoanhThu:N0} VND"));
                row.RelativeItem().Element(e => Card(e, "Người mới", $"{model.Users.SoNguoiMoi:N0}"));
                row.RelativeItem().Element(e => Card(e, "Khóa học mới", $"{model.Courses.SoKhoaHocMoi:N0}"));
                row.RelativeItem().Element(e => Card(e, "Lượt thi LT", $"{model.Tests.TongLuotThiLyThuyet:N0}"));
            });
        }

        private static void Card(IContainer container, string title, string value)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(8)
                .Column(col =>
                {
                    col.Item().Text(title).FontSize(9).FontColor(Colors.Grey.Darken2);
                    col.Item().Text(value).FontSize(14).Bold();
                });
        }

        // ======================================================
        // ===================== PDF: CHARTS ====================
        // ======================================================
        private static void BuildChartsFromView(ColumnDescriptor col, List<ChartImageDto> charts)
        {
            if (charts == null || charts.Count == 0)
            {
                col.Item().Text("Không có biểu đồ để hiển thị (client chưa gửi ảnh biểu đồ).")
                    .FontColor(Colors.Grey.Darken1);
                return;
            }

            foreach (var chart in charts)
            {
                if (string.IsNullOrWhiteSpace(chart?.ImageBase64))
                    continue;

                col.Item().Column(c =>
                {
                    c.Spacing(6);

                    c.Item().Text((chart.Name ?? "BIỂU ĐỒ")
                            .Replace("_", " ")
                            .ToUpper())
                        .Bold()
                        .FontSize(13);

                    var base64 = chart.ImageBase64;
                    if (base64.Contains(","))
                        base64 = base64.Split(',')[1];

                    byte[] bytes;
                    try
                    {
                        bytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        c.Item()
                            .Text("Biểu đồ lỗi dữ liệu (base64 không hợp lệ).")
                            .FontColor(Colors.Red.Darken2);
                        return;
                    }

                    if (bytes.Length < 8 ||
                        bytes[0] != 0x89 ||
                        bytes[1] != 0x50 ||
                        bytes[2] != 0x4E ||
                        bytes[3] != 0x47)
                    {
                        c.Item()
                            .Text("Biểu đồ lỗi dữ liệu (PNG không hợp lệ).")
                            .FontColor(Colors.Red.Darken2);
                        return;
                    }

                    c.Item()
                        .Image(bytes)
                        .FitWidth();

                    c.Item()
                        .PaddingBottom(8)
                        .Text(
                            string.IsNullOrWhiteSpace(chart.Note)
                                ? "Biểu đồ được chụp từ giao diện theo bộ lọc hiện tại."
                                : chart.Note
                        )
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken2);
                });
            }
        }

        // ======================================================
        // ===================== PDF: TABLES ====================
        // ======================================================
        private static void BuildOverviewTables(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Bảng chi tiết doanh thu theo tháng").Bold();
            col.Item().Element(c => BuildRevenueTable(c, model.Overview.RevenueByMonthTable));
        }

        private static void BuildUsersTables(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Bảng chi tiết người dùng theo tháng").Bold();
            col.Item().Element(c => BuildUsersTable(c, model.Users.UsersByMonthTable));
        }

        private static void BuildCoursesTables(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Bảng chi tiết khóa học & đăng ký theo tháng").Bold();
            col.Item().Element(c => BuildCoursesTable(c, model.Courses.CoursesByMonthTable));

            col.Item().PaddingTop(8).Text("Phân bố khóa học theo hạng").Bold();
            col.Item().Element(c => BuildPieLikeTable(c, model.Courses.CoursesByHangPie));
        }

        private static void BuildTestsTables(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Phân bố bài thi lý thuyết theo hạng").Bold();
            col.Item().Element(c => BuildPieLikeTable(c, model.Tests.TheoryByHangPie));

            col.Item().PaddingTop(8).Text("Phân bố bài thi mô phỏng theo bộ đề").Bold();
            col.Item().Element(c => BuildPieLikeTable(c, model.Tests.SimulationByBoDePie));

            col.Item().PaddingTop(8)
                .Text("Ghi chú: Biểu đồ theo tháng của bài thi cần cột thời gian làm bài (CreatedAt/NgayLam).")
                .FontSize(9)
                .FontColor(Colors.Grey.Darken2);
        }

        // ===================== TABLE HELPERS =====================
        private static void BuildRevenueTable(IContainer container, List<RevenueMonthRowVM> rows)
        {
            rows ??= new();

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Tháng");
                    HeaderCell(h.Cell(), "Tổng (VND)");
                    HeaderCell(h.Cell(), "Khóa học");
                    HeaderCell(h.Cell(), "Thuê xe");
                    HeaderCell(h.Cell(), "Giao dịch");
                });

                foreach (var r in rows.Take(12))
                {
                    BodyCell(table.Cell(), r.MonthLabel);
                    BodyCell(table.Cell(), r.RevenueTotal.ToString("N0"));
                    BodyCell(table.Cell(), r.RevenueCourses.ToString("N0"));
                    BodyCell(table.Cell(), r.RevenueVehicles.ToString("N0"));
                    BodyCell(table.Cell(), r.Transactions.ToString("N0"));
                }
            });
        }

        private static void BuildUsersTable(IContainer container, List<UserMonthRowVM> rows)
        {
            rows ??= new();

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Tháng");
                    HeaderCell(h.Cell(), "Người mới");
                    HeaderCell(h.Cell(), "Hồ sơ mới");
                });

                foreach (var r in rows.Take(12))
                {
                    BodyCell(table.Cell(), r.MonthLabel);
                    BodyCell(table.Cell(), r.NewUsers.ToString("N0"));
                    BodyCell(table.Cell(), r.NewProfiles.ToString("N0"));
                }
            });
        }

        private static void BuildCoursesTable(IContainer container, List<CourseMonthRowVM> rows)
        {
            rows ??= new();

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Tháng");
                    HeaderCell(h.Cell(), "Khóa học mới");
                    HeaderCell(h.Cell(), "Đăng ký mới");
                });

                foreach (var r in rows.Take(12))
                {
                    BodyCell(table.Cell(), r.MonthLabel);
                    BodyCell(table.Cell(), r.NewCourses.ToString("N0"));
                    BodyCell(table.Cell(), r.NewEnrollments.ToString("N0"));
                }
            });
        }

        private static void BuildPieLikeTable(IContainer container, List<PieSliceVM> rows)
        {
            rows ??= new();

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Danh mục");
                    HeaderCell(h.Cell(), "Số lượng");
                });

                foreach (var r in rows.Take(12))
                {
                    BodyCell(table.Cell(), r.Label);
                    BodyCell(table.Cell(), r.Value.ToString("N0"));
                }
            });
        }

        private static void HeaderCell(IContainer cell, string text)
        {
            cell.Background(Colors.Grey.Lighten3)
                .Padding(6)
                .Text(text)
                .SemiBold();
        }

        private static void BodyCell(IContainer cell, string text)
        {
            cell.BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .Text(text);
        }

        // ======================================================
        // ===================== HELPERS ========================
        // ======================================================
        private static DateOnly? ToDateOnly(DateTime? dt)
            => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null;

        private static string FormatRange(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue) return "Tất cả";
            if (fromDate.HasValue && toDate.HasValue) return $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";
            if (fromDate.HasValue) return $"Từ {fromDate:dd/MM/yyyy}";
            return $"Đến {toDate:dd/MM/yyyy}";
        }

        private static string TabTitle(string tab) => (tab ?? "overview").ToLower() switch
        {
            "users" => "BÁO CÁO NGƯỜI DÙNG",
            "courses" => "BÁO CÁO KHÓA HỌC",
            "tests" => "BÁO CÁO BÀI THI",
            _ => "BÁO CÁO TỔNG QUAN"
        };

        private static (int Year, int Month) ParseMonthKey(string label)
        {
            var parts = (label ?? "").Split('/');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var m) &&
                int.TryParse(parts[1], out var y))
            {
                return (y, m);
            }
            return (0, 0);
        }
    }
}
