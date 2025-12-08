using System.Collections.Generic;

namespace dacn_dtgplx.ViewModels
{
    public class ExamViewModel
    {
        public int IdBoDe { get; set; }
        public string TenBoDe { get; set; } = "";
        public string Hang { get; set; } = "";

        public int ThoiGian { get; set; }      // phút
        public int TongCau { get; set; }
        public int DiemDat { get; set; }

        public List<ExamQuestionVM> CauHoi { get; set; } = new List<ExamQuestionVM>();

        // ====== TRẠNG THÁI KHI NỘP BÀI ======
        public bool IsSubmitted { get; set; }
        public int SoCauDung { get; set; }
        public int SoCauSai { get; set; }
        public bool CoCauLietSai { get; set; }
        public bool Dat { get; set; }
        public int ThoiGianLam { get; set; }   // giây

        // IdCauHoi -> IdDapAn đã chọn (hoặc null)
        public Dictionary<int, int?> DapAnDaChon { get; set; } = new Dictionary<int, int?>();
        public bool IsRandomExam { get; set; } = false;
        public List<string> DanhSachMeo { get; set; } = new();
    }
}
