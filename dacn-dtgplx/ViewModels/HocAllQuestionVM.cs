namespace dacn_dtgplx.ViewModels
{
    public class HocAllQuestionVM
    {
        // STT toàn bộ (1,2,3,...)
        public int GlobalIndex { get; set; }

        public int IdCauHoi { get; set; }
        public string? NoiDung { get; set; }

        // Đường dẫn hình câu hỏi (đã chuẩn hoá, bỏ wwwroot/)
        public string? ImageUrl { get; set; }

        public bool IsCauLiet { get; set; }
        public bool IsChuY { get; set; }
        public bool IsXeMay { get; set; }

        // Hình mẹo (đã chuẩn hoá, bỏ wwwroot/)
        public string? UrlAnhMeo { get; set; }

        public List<HocAllAnswerVM> DapAns { get; set; } = new();
    }
}
