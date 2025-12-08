using System.Collections.Generic;

namespace dacn_dtgplx.ViewModels
{
    public class ExamQuestionVM
    {
        public int IdCauHoi { get; set; }
        public string NoiDung { get; set; } = "";
        public string? ImageUrl { get; set; }   // đã được xử lý bỏ "wwwroot"
        public bool LaCauLiet { get; set; }

        public List<ExamAnswerVM> DapAn { get; set; } = new List<ExamAnswerVM>();
    }
}
