namespace dacn_dtgplx.ViewModels
{
    public class BienBaoFlashStudyPageVM
    {
        public bool IsLoggedIn { get; set; }
        public string LoginUrl { get; set; } = "/Auth/Login"; // sửa theo route của bạn
        public List<BienBaoFlashCardVM> Cards { get; set; } = new();
    }
}
