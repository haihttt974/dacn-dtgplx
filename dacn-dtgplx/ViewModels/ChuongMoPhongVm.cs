namespace dacn_dtgplx.ViewModels
{
    public class ChuongMoPhongVm
    {
        public int IdChuongMp { get; set; }
        public string TenChuong { get; set; } = "";
        public List<TinhHuongItem2> TinhHuongs { get; set; } = new();
    }
}
