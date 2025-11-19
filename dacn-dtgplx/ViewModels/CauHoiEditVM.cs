using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class CauHoiEditVM
    {
        public int IdCauHoi { get; set; }
        public int ChuongId { get; set; }
        public string? NoiDung { get; set; }
        public bool CauLiet { get; set; }
        public bool ChuY { get; set; }
        public bool XeMay { get; set; }

        public string? HinhAnh { get; set; }
        public string? UrlAnhMeo { get; set; }

        public IFormFile? UploadHinhAnh { get; set; }
        public IFormFile? UploadAnhMeo { get; set; }

        public List<DapAnVM> DapAns { get; set; } = new();
    }
}
