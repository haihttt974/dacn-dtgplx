using dacn_dtgplx.Models;
using System.Collections.Generic;

namespace dacn_dtgplx.ViewModels
{
    public class CauHoiIndexVM
    {
        public List<CauHoiLyThuyet> Items { get; set; } = new();

        // Filter hiện tại
        public CauHoiFilter Filter { get; set; } = new();

        // Danh sách chương để hiển thị dropdown
        public List<Chuong> Chapters { get; set; } = new();

        // Phân trang
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    }
}
