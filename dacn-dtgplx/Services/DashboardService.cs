using dacn_dtgplx.DTOs;
using dacn_dtgplx.Models;
using dacn_dtgplx.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Services
{
    public class DashboardService
    {
        private readonly DtGplxContext _db;

        public DashboardService(DtGplxContext db)
        {
            _db = db;
        }

        public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
        {
            // ===== USER =====
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return new UserDashboardDto();

            var dto = new UserDashboardDto
            {
                UserId = user.UserId,
                FullName = user.TenDayDu,
                Avatar = user.Avatar,
                Email = user.Email,
                RoleName = user.Role?.RoleName,
                CreatedAt = user.TaoLuc,
                LastLogin = user.LanDangNhapGanNhat
            };

            // ===== HỒ SƠ =====
            var hoSo = await _db.HoSoThiSinhs
                .Include(h => h.DangKyHocs)
                    .ThenInclude(d => d.KhoaHoc)
                        .ThenInclude(k => k.IdHangNavigation)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hoSo != null)
            {
                // lấy 1 đăng ký gần nhất để suy ra hạng
                var dkGanNhat = hoSo.DangKyHocs
                    .OrderByDescending(d => d.IdDangKy)
                    .FirstOrDefault();

                dto.HoSo = new HoSoDashboardDto
                {
                    HoSoId = hoSo.HoSoId,
                    HangTen = dkGanNhat?.KhoaHoc?.IdHangNavigation?.TenDayDu,
                    DaDuyet = hoSo.DaDuyet ?? false
                };
            }

            // ===== TIẾN ĐỘ =====
            if (hoSo != null)
            {
                var tienDo = await _db.KetQuaHocTaps
                    .FirstOrDefaultAsync(x => x.HoSoId == hoSo.HoSoId);

                if (tienDo != null)
                {
                    dto.TienDo = new TienDoDashboardDto
                    {
                        HtLyThuyet = tienDo.HtLyThuyet,
                        HtMoPhong = tienDo.HtMoPhong,
                        HtSaHinh = tienDo.HtSaHinh,
                        HtDuongTruong = tienDo.HtDuongTruong,
                        DuDkThiTn = tienDo.DuDkThiTn,
                        DuDkThiSh = tienDo.DuDkThiSh
                    };
                }
            }

            // ===== LỊCH HỌC =====
            dto.LichHoc = await _db.LichHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.XeTapLai)
                .Where(l => l.KhoaHoc.DangKyHocs.Any(d => d.HoSo.UserId == userId))
                .OrderBy(l => l.NgayHoc)
                .Take(10)
                .Select(l => new LichHocDashboardDto
                {
                    NgayHoc = l.NgayHoc,                 // DateOnly -> DateOnly (đúng loại)
                    NoiDung = l.NoiDung,
                    DiaDiem = l.DiaDiem,
                    LoaiXe = l.XeTapLai != null ? l.XeTapLai.LoaiXe : null,
                    TenKhoaHoc = l.KhoaHoc.TenKhoaHoc
                })
                .ToListAsync();

            return dto;
        }
    }
}
