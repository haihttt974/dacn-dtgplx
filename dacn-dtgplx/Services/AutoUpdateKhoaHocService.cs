using dacn_dtgplx.Models;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Services
{
    public class AutoUpdateKhoaHocService
    {
        private readonly DtGplxContext _context;

        public AutoUpdateKhoaHocService(DtGplxContext context)
        {
            _context = context;
        }

        public async Task UpdateKhoaHocStatusAsync()
        {
            var now = DateTime.Now;

            var khoaHocs = await _context.KhoaHocs
                .Include(k => k.LichHocs)
                .ToListAsync();

            foreach (var kh in khoaHocs)
            {
                bool shouldDeactivate = false;

                // RULE 1: Ngày kết thúc < hiện tại
                var today = DateTime.Today; // chỉ lấy phần ngày

                if (kh.NgayKetThuc.HasValue && kh.NgayKetThuc.Value.Date < today)
                {
                    shouldDeactivate = true;
                }

                // RULE 2: Không còn lịch học nào hiện tại hoặc tương lai
                bool hasFutureSchedule = kh.LichHocs.Any(l =>
                {
                    var endDateTime = new DateTime(
                        l.NgayHoc.Year,
                        l.NgayHoc.Month,
                        l.NgayHoc.Day,
                        l.TgKetThuc.Hour,
                        l.TgKetThuc.Minute,
                        l.TgKetThuc.Second
                    );

                    return endDateTime >= now;  // vẫn đang học hoặc chưa học
                });

                if (!hasFutureSchedule)
                    shouldDeactivate = true;

                // APPLY
                if (shouldDeactivate)
                    kh.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}
