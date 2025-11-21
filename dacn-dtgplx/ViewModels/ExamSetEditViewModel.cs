using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class ExamSetEditViewModel
    {
        // ===== THÔNG TIN BỘ ĐỀ =====
        public int IdBoDe { get; set; }

        [Required(ErrorMessage = "Tên bộ đề không được để trống")]
        [Display(Name = "Tên bộ đề")]
        public string? TenBoDe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạng GPLX")]
        [Display(Name = "Hạng GPLX")]
        public int IdHang { get; set; }

        [Display(Name = "Thời gian làm bài (phút)")]
        public int? ThoiGian { get; set; }

        [Display(Name = "Số câu hỏi")]
        public int? SoCauHoi { get; set; }

        [Display(Name = "Hoạt động")]
        public bool HoatDong { get; set; } = true;
        public int? MaxQuestions { get; set; }

        // ===== DANH SÁCH CÂU HỎI TRONG ĐỀ =====
        /// <summary>
        /// Danh sách ID câu hỏi theo thứ tự trong bộ đề.
        /// Đây là property mà controller & view Edit đang dùng.
        /// </summary>
        public List<int> SelectedQuestionIds { get; set; } = new List<int>();

        /// <summary>
        /// Danh sách chi tiết bộ đề hiện tại (dùng nếu cần truy thêm thông tin).
        /// </summary>
        public List<ChiTietBoDeTn> CurrentQuestions { get; set; } = new List<ChiTietBoDeTn>();

        // ===== DỮ LIỆU PHỤ CHO DROPDOWN =====
        public List<Hang> Hangs { get; set; } = new List<Hang>();

        public List<Chuong> Chuongs { get; set; } = new List<Chuong>();

        /// <summary>
        /// Tất cả câu hỏi (đã Include Chuong) để dùng cho dropdown chọn câu hỏi.
        /// </summary>
        public List<CauHoiLyThuyet> AllQuestions { get; set; } = new List<CauHoiLyThuyet>();

        // (tùy chọn nếu sau này cần binding thêm)
        public int? SelectedChapterId { get; set; }
        public int? SelectedNewQuestionId { get; set; }
    }
}
