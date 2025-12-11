using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class LichDayViewModel
    {
        public TtGiaoVien GiaoVien { get; set; }

        public string Mode { get; set; } = "day";

        public DateTime CurrentDate { get; set; } = DateTime.Today;

        public List<LichDayItem> LichDayItems { get; set; } = new();
    }
}
