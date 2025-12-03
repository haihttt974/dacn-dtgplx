using System.Collections.Generic;
using dacn_dtgplx.Models;   // dùng các entity trong DbContext

namespace dacn_dtgplx.ViewModels   // CHÚ Ý: ViewModels (có s) + đúng root namespace
{
    public class HomeViewModel
    {
        // Dùng cho phần "Khóa Học"
        public List<KhoaHoc> Courses { get; set; } = new();

        // Dùng cho phần "Giảng Viên"
        public List<TtGiaoVien> Instructors { get; set; } = new();

        // Dùng cho phần "Ý Kiến Học Viên"
        public List<PhanHoi> Testimonials { get; set; } = new();
    }
}
