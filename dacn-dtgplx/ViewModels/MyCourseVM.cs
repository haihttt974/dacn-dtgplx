using dacn_dtgplx.Models;

namespace dacn_dtgplx.ViewModels
{
    public class MyCourseVM
    {
        public DangKyHoc DangKy { get; set; }
        public KhoaHoc KhoaHoc { get; set; }
        public User? GiaoVien { get; set; }   // GV phụ trách nếu tìm được
    }
}
