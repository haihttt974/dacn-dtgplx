namespace dacn_dtgplx.ViewModels.Reports
{
    public class AdminReportIndexVM
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }

        // KPI
        public decimal TongDoanhThu { get; set; }
        public int TongHoaDon { get; set; }
        public int TongHocVien { get; set; }
        public int TongKhoaHoc { get; set; }

        // Charts
        public List<TimeValueVM> DoanhThuNgay { get; set; } = new();
        public List<TimeValueVM> DoanhThuTuan { get; set; } = new();
        public List<TimeValueVM> DoanhThuThang { get; set; } = new();
        public List<TimeValueVM> DoanhThuQuy { get; set; } = new();

        public List<HangValueVM> DoanhThuTheoHang { get; set; } = new();
        public List<HangValueVM> HocVienTheoHang { get; set; } = new();
    }
}
