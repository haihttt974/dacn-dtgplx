using dacn_dtgplx.ViewModels.Report;

namespace dacn_dtgplx.ViewModels.Reports
{
    public class CoursesSectionVM
    {
        // KPI
        public int SoKhoaHocMoi { get; set; }                  // theo NgayBatDau trong filter
        public int SoDangKyMoi { get; set; }                   // DangKyHoc.NgayDangKy trong filter
        public int TongDangKy { get; set; }                    // tổng đăng ký (tuỳ muốn)

        // Charts
        public List<ChartPointVM> NewCoursesByMonth { get; set; } = new();
        public List<ChartPointVM> NewEnrollmentsByMonth { get; set; } = new(); // đăng ký theo tháng
        public List<PieSliceVM> CoursesByHangPie { get; set; } = new();

        // Tables
        public List<CourseMonthRowVM> CoursesByMonthTable { get; set; } = new();

        // Top khóa học theo số đăng ký
        public List<PieSliceVM> TopCoursesByEnrollments { get; set; } = new();
    }

}
