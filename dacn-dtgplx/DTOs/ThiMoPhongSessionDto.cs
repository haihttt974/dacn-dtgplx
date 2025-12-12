using dacn_dtgplx.ViewModels;

namespace dacn_dtgplx.DTOs
{
    public class ThiMoPhongSessionDto
    {
        public int IdBoDe { get; set; }
        public List<FlagItem> Flags { get; set; } = new();
        public int TongDiem { get; set; }
    }
}
