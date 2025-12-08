namespace dacn_dtgplx.ViewModels
{
    public class HocAllViewModel
    {
        public string SelectedHang { get; set; } = "";
        public bool IsXeMay { get; set; }

        public int TotalQuestions { get; set; }
        public int TotalChapters { get; set; }

        // Danh sách chương + câu hỏi
        public List<HocAllChapterVM> Chapters { get; set; } = new();
    }
}
