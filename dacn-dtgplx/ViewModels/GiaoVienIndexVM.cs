namespace dacn_dtgplx.ViewModels
{
    public class GiaoVienIndexVM
    {
        public int Id { get; set; }
        public string Ten { get; set; }
        public string ChuyenMon { get; set; }
        public List<string> ChuyenDaoTao { get; set; } = new();
        public DateOnly? NgayVaoLam { get; set; }
        public List<int> LichDay { get; set; } = new();
    }
}
