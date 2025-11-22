using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class SimulationExamSetManageViewModel
    {
        public int IdBoDe { get; set; }
        public string? TenBoDe { get; set; }

        // Danh sách 10 TH đầy đủ thông tin
        public List<SelectedSituationVM> SelectedDetails { get; set; } = new();

        // Dữ liệu dùng cho Random
        public List<ChuongMoPhong> AllChapters { get; set; } = new();

        // Toàn bộ TH trong hệ thống để filter random
        public List<TinhHuongMoPhong> AllSituations { get; set; } = new();
    }
}
