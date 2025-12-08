namespace dacn_dtgplx.ViewModels
{
    public class HocAllChapterVM
    {
        public int ChuongId { get; set; }
        public string? TenChuong { get; set; }
        public int ThuTu { get; set; }

        // Các câu hỏi thuộc chương này
        public List<HocAllQuestionVM> Questions { get; set; } = new();
    }
}
