namespace dacn_dtgplx.ViewModels
{
    public class ExamAnswerVM
    {
        public int IdDapAn { get; set; }
        public string Label { get; set; } = ""; // ví dụ "1", "2", "3", "4"
        public bool IsCorrect { get; set; }
    }
}
