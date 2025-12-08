using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace dacn_dtgplx.Models;

public partial class DtGplxContext : DbContext
{
    public DtGplxContext()
    {
    }

    public DtGplxContext(DbContextOptions<DtGplxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiLam> BaiLams { get; set; }

    public virtual DbSet<BaiLamMoPhong> BaiLamMoPhongs { get; set; }

    public virtual DbSet<BienBao> BienBaos { get; set; }

    public virtual DbSet<BoDeMoPhong> BoDeMoPhongs { get; set; }

    public virtual DbSet<BoDeThiThu> BoDeThiThus { get; set; }

    public virtual DbSet<CauHoiLyThuyet> CauHoiLyThuyets { get; set; }

    public virtual DbSet<ChiTietBaiLam> ChiTietBaiLams { get; set; }

    public virtual DbSet<ChiTietBoDeMoPhong> ChiTietBoDeMoPhongs { get; set; }

    public virtual DbSet<ChiTietBoDeTn> ChiTietBoDeTns { get; set; }

    public virtual DbSet<Chuong> Chuongs { get; set; }

    public virtual DbSet<ChuongMoPhong> ChuongMoPhongs { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<CtThongBao> CtThongBaos { get; set; }

    public virtual DbSet<DangKyHoc> DangKyHocs { get; set; }

    public virtual DbSet<DapAn> DapAns { get; set; }

    public virtual DbSet<DiemTungTinhHuong> DiemTungTinhHuongs { get; set; }

    public virtual DbSet<FlashCard> FlashCards { get; set; }

    public virtual DbSet<Hang> Hangs { get; set; }

    public virtual DbSet<HoSoThiSinh> HoSoThiSinhs { get; set; }

    public virtual DbSet<HoaDonThanhToan> HoaDonThanhToans { get; set; }

    public virtual DbSet<KetQuaHocTap> KetQuaHocTaps { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHocs { get; set; }

    public virtual DbSet<LichHoc> LichHocs { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<PhanHoi> PhanHois { get; set; }

    public virtual DbSet<QuyDinhHang> QuyDinhHangs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TinhHuongMoPhong> TinhHuongMoPhongs { get; set; }

    public virtual DbSet<TtGiaoVien> TtGiaoViens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WebsocketConnection> WebsocketConnections { get; set; }

    public virtual DbSet<XeTapLai> XeTapLais { get; set; }

    public virtual DbSet<PhieuThueXe> PhieuThueXe { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=haiit;Database=DT_GPLX;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiLam>(entity =>
        {
            entity.HasKey(e => e.BaiLamId).HasName("PK__baiLam__E508357B9C93413A");

            entity.ToTable("baiLam");

            entity.HasIndex(e => e.IdBoDe, "IX_BaiLam_BoDe");

            entity.HasIndex(e => e.UserId, "IX_BaiLam_User");

            entity.Property(e => e.BaiLamId).HasColumnName("baiLamId");
            entity.Property(e => e.IdBoDe).HasColumnName("idBoDe");
            entity.Property(e => e.KetQua).HasColumnName("ketQua");
            entity.Property(e => e.SoCauSai)
                .HasDefaultValue(0)
                .HasColumnName("soCauSai");
            entity.Property(e => e.ThoiGianLamBai).HasColumnName("thoiGianLamBai");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.IdBoDeNavigation).WithMany(p => p.BaiLams)
                .HasForeignKey(d => d.IdBoDe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiLam_BoDeThiThu");

            entity.HasOne(d => d.User).WithMany(p => p.BaiLams)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiLam_User");
        });

        modelBuilder.Entity<BaiLamMoPhong>(entity =>
        {
            entity.HasKey(e => e.IdBaiLamTongDiem).HasName("PK__baiLamMo__95A422BF3FF2DC48");

            entity.ToTable("baiLamMoPhong");

            entity.HasIndex(e => e.IdBoDeMoPhong, "IX_BaiLamMoPhong_BoDeMp");

            entity.HasIndex(e => e.UserId, "IX_BaiLamMoPhong_User");

            entity.Property(e => e.IdBaiLamTongDiem).HasColumnName("idBaiLamTongDiem");
            entity.Property(e => e.IdBoDeMoPhong).HasColumnName("idBoDeMoPhong");
            entity.Property(e => e.KetQua).HasColumnName("ketQua");
            entity.Property(e => e.TongDiem)
                .HasDefaultValue(0)
                .HasColumnName("tongDiem");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.IdBoDeMoPhongNavigation).WithMany(p => p.BaiLamMoPhongs)
                .HasForeignKey(d => d.IdBoDeMoPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiLamMoPhong_BoDeMoPhong");

            entity.HasOne(d => d.User).WithMany(p => p.BaiLamMoPhongs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiLamMoPhong_User");
        });

        modelBuilder.Entity<BienBao>(entity =>
        {
            entity.HasKey(e => e.IdBienBao).HasName("PK__BienBao__D4FA89D5D8203A67");

            entity.ToTable("BienBao");

            entity.Property(e => e.HinhAnh).HasMaxLength(500);
            entity.Property(e => e.TenBienBao).HasMaxLength(255);
            entity.Property(e => e.Ynghia).HasColumnName("YNghia");
        });

        modelBuilder.Entity<BoDeMoPhong>(entity =>
        {
            entity.HasKey(e => e.IdBoDeMoPhong).HasName("PK__boDeMoPh__97120771CB811E2B");

            entity.ToTable("boDeMoPhong");

            entity.Property(e => e.IdBoDeMoPhong).HasColumnName("idBoDeMoPhong");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.SoTinhHuong).HasColumnName("soTinhHuong");
            entity.Property(e => e.TaoLuc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("taoLuc");
            entity.Property(e => e.TenBoDe)
                .HasMaxLength(255)
                .HasColumnName("tenBoDe");
        });

        modelBuilder.Entity<BoDeThiThu>(entity =>
        {
            entity.HasKey(e => e.IdBoDe).HasName("PK__boDeThiT__D80462AFCFEE92F6");

            entity.ToTable("boDeThiThu");

            entity.HasIndex(e => e.IdHang, "IX_BoDeThiThu_Hang");

            entity.Property(e => e.IdBoDe).HasColumnName("idBoDe");
            entity.Property(e => e.HoatDong)
                .HasDefaultValue(true)
                .HasColumnName("hoatDong");
            entity.Property(e => e.IdHang).HasColumnName("idHang");
            entity.Property(e => e.SoCauHoi).HasColumnName("soCauHoi");
            entity.Property(e => e.TaoLuc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("taoLuc");
            entity.Property(e => e.TenBoDe)
                .HasMaxLength(255)
                .HasColumnName("tenBoDe");
            entity.Property(e => e.ThoiGian).HasColumnName("thoiGian");

            entity.HasOne(d => d.IdHangNavigation).WithMany(p => p.BoDeThiThus)
                .HasForeignKey(d => d.IdHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BoDeThiThu_Hang");
        });

        modelBuilder.Entity<CauHoiLyThuyet>(entity =>
        {
            entity.HasKey(e => e.IdCauHoi).HasName("PK__cauHoiLy__205222729145CAFC");

            entity.ToTable("cauHoiLyThuyet");

            entity.HasIndex(e => e.ChuongId, "IX_CauHoiLyThuyet_Chuong");

            entity.Property(e => e.IdCauHoi).HasColumnName("idCauHoi");
            entity.Property(e => e.CauLiet).HasColumnName("cauLiet");
            entity.Property(e => e.ChuY).HasColumnName("chuY");
            entity.Property(e => e.ChuongId).HasColumnName("chuongId");
            entity.Property(e => e.HinhAnh)
                .HasMaxLength(500)
                .HasColumnName("hinhAnh");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.UrlAnhMeo)
                .HasMaxLength(500)
                .HasColumnName("urlAnhMeo");
            entity.Property(e => e.XeMay).HasColumnName("xeMay");

            entity.HasOne(d => d.Chuong).WithMany(p => p.CauHoiLyThuyets)
                .HasForeignKey(d => d.ChuongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CauHoiLyThuyet_Chuong");
        });

        modelBuilder.Entity<ChiTietBaiLam>(entity =>
        {
            entity.HasKey(e => new { e.BaiLamId, e.IdCauHoi }).HasName("PK__chiTietB__D70D175C27397488");

            entity.ToTable("chiTietBaiLam");

            entity.HasIndex(e => e.BaiLamId, "IX_ChiTietBaiLam_BaiLam");

            entity.Property(e => e.BaiLamId).HasColumnName("baiLamId");
            entity.Property(e => e.IdCauHoi).HasColumnName("idCauHoi");
            entity.Property(e => e.DapAnDaChon)
                .HasMaxLength(50)
                .HasColumnName("dapAnDaChon");
            entity.Property(e => e.KetQuaCau).HasColumnName("ketQuaCau");

            entity.HasOne(d => d.BaiLam).WithMany(p => p.ChiTietBaiLams)
                .HasForeignKey(d => d.BaiLamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietBaiLam_BaiLam");

            entity.HasOne(d => d.IdCauHoiNavigation).WithMany(p => p.ChiTietBaiLams)
                .HasForeignKey(d => d.IdCauHoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietBaiLam_CauHoiLyThuyet");
        });

        modelBuilder.Entity<ChiTietBoDeMoPhong>(entity =>
        {
            entity.HasKey(e => new { e.IdBoDeMoPhong, e.IdThMp }).HasName("PK__chiTietB__2CDBA8EEF3C82FCD");

            entity.ToTable("chiTietBoDeMoPhong");

            entity.HasIndex(e => e.IdBoDeMoPhong, "IX_ChiTietBoDeMoPhong_BoDe");

            entity.HasIndex(e => e.IdThMp, "IX_ChiTietBoDeMoPhong_ThMp");

            entity.Property(e => e.IdBoDeMoPhong).HasColumnName("idBoDeMoPhong");
            entity.Property(e => e.IdThMp).HasColumnName("idThMp");
            entity.Property(e => e.ThuTu).HasColumnName("thuTu");

            entity.HasOne(d => d.IdBoDeMoPhongNavigation).WithMany(p => p.ChiTietBoDeMoPhongs)
                .HasForeignKey(d => d.IdBoDeMoPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTBoDeMoPhong_BoDeMoPhong");

            entity.HasOne(d => d.IdThMpNavigation).WithMany(p => p.ChiTietBoDeMoPhongs)
                .HasForeignKey(d => d.IdThMp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTBoDeMoPhong_TinhHuongMoPhong");
        });

        modelBuilder.Entity<ChiTietBoDeTn>(entity =>
        {
            entity.HasKey(e => new { e.IdBoDe, e.IdCauHoi }).HasName("PK__chiTietB__EA014088C4E6F20D");

            entity.ToTable("chiTietBoDeTN");

            entity.HasIndex(e => e.IdBoDe, "IX_ChiTietBoDeTN_BoDe");

            entity.HasIndex(e => e.IdCauHoi, "IX_ChiTietBoDeTN_CauHoi");

            entity.Property(e => e.IdBoDe).HasColumnName("idBoDe");
            entity.Property(e => e.IdCauHoi).HasColumnName("idCauHoi");
            entity.Property(e => e.ThuTu)
                .HasDefaultValue(0)
                .HasColumnName("thuTu");

            entity.HasOne(d => d.IdBoDeNavigation).WithMany(p => p.ChiTietBoDeTns)
                .HasForeignKey(d => d.IdBoDe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietBoDeTN_BoDeThiThu");

            entity.HasOne(d => d.IdCauHoiNavigation).WithMany(p => p.ChiTietBoDeTns)
                .HasForeignKey(d => d.IdCauHoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietBoDeTN_CauHoiLyThuyet");
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.HasKey(e => e.ChuongId).HasName("PK__chuong__DA18D025D3818C48");

            entity.ToTable("chuong");

            entity.Property(e => e.ChuongId).HasColumnName("chuongId");
            entity.Property(e => e.TenChuong)
                .HasMaxLength(255)
                .HasColumnName("tenChuong");
            entity.Property(e => e.ThuTu).HasColumnName("thuTu");
        });

        modelBuilder.Entity<ChuongMoPhong>(entity =>
        {
            entity.HasKey(e => e.IdChuongMp).HasName("PK__chuongMo__DE0A874F50EA73E0");

            entity.ToTable("chuongMoPhong");

            entity.Property(e => e.IdChuongMp).HasColumnName("idChuongMp");
            entity.Property(e => e.TenChuong)
                .HasMaxLength(255)
                .HasColumnName("tenChuong");
            entity.Property(e => e.ThuTu).HasColumnName("thuTu");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationsId).HasName("PK__conversa__3E3D3B8064D788F1");

            entity.ToTable("conversations");

            entity.Property(e => e.ConversationsId).HasColumnName("conversationsId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.LastMessageAt)
                .HasPrecision(3)
                .HasColumnName("lastMessageAt");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserId2).HasColumnName("userId2");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_User1");

            entity.HasOne(d => d.UserId2Navigation).WithMany(p => p.ConversationUserId2Navigations)
                .HasForeignKey(d => d.UserId2)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_User2");
        });

        modelBuilder.Entity<CtThongBao>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ThongBaoId }).HasName("PK__ctThongB__E7F50D4DC63C19A1");

            entity.ToTable("ctThongBao");

            entity.HasIndex(e => e.ThongBaoId, "IX_CtThongBao_ThongBao");

            entity.HasIndex(e => e.UserId, "IX_CtThongBao_User");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.ThongBaoId).HasColumnName("thongBaoId");
            entity.Property(e => e.DaXem).HasColumnName("daXem");
            entity.Property(e => e.ThoiGianGui)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoiGianGui");

            entity.HasOne(d => d.ThongBao).WithMany(p => p.CtThongBaos)
                .HasForeignKey(d => d.ThongBaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CtThongBao_ThongBao");

            entity.HasOne(d => d.User).WithMany(p => p.CtThongBaos)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CtThongBao_User");
        });

        modelBuilder.Entity<DangKyHoc>(entity =>
        {
            entity.HasKey(e => e.IdDangKy).HasName("PK__dangKyHo__AE481C76BEDDF257");

            entity.ToTable("dangKyHoc");

            entity.HasIndex(e => e.HoSoId, "IX_DangKyHoc_HoSo");

            entity.HasIndex(e => e.KhoaHocId, "IX_DangKyHoc_KhoaHoc");

            entity.Property(e => e.IdDangKy).HasColumnName("idDangKy");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.HoSoId).HasColumnName("hoSoId");
            entity.Property(e => e.KhoaHocId).HasColumnName("khoaHocId");
            entity.Property(e => e.NgayDangKy).HasColumnName("ngayDangKy");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.HoSo).WithMany(p => p.DangKyHocs)
                .HasForeignKey(d => d.HoSoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKyHoc_HoSoThiSinh");

            entity.HasOne(d => d.KhoaHoc).WithMany(p => p.DangKyHocs)
                .HasForeignKey(d => d.KhoaHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKyHoc_KhoaHoc");
        });

        modelBuilder.Entity<DapAn>(entity =>
        {
            entity.HasKey(e => e.IdDapAn).HasName("PK__dapAn__B1FBEE8ABB6FC5D3");

            entity.ToTable("dapAn");

            entity.HasIndex(e => e.IdCauHoi, "IX_DapAn_CauHoi");

            entity.Property(e => e.IdDapAn).HasColumnName("idDapAn");
            entity.Property(e => e.DapAnDung).HasColumnName("dapAnDung");
            entity.Property(e => e.IdCauHoi).HasColumnName("idCauHoi");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.ThuTu).HasColumnName("thuTu");

            entity.HasOne(d => d.IdCauHoiNavigation).WithMany(p => p.DapAns)
                .HasForeignKey(d => d.IdCauHoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DapAn_CauHoiLyThuyet");
        });

        modelBuilder.Entity<DiemTungTinhHuong>(entity =>
        {
            entity.HasKey(e => new { e.IdBaiLamTongDiem, e.IdThMp }).HasName("PK__diemTung__2E6D8D205210F512");

            entity.ToTable("diemTungTinhHuong");

            entity.HasIndex(e => e.IdBaiLamTongDiem, "IX_DiemTungTinhHuong_BaiLamMp");

            entity.HasIndex(e => e.IdThMp, "IX_DiemTungTinhHuong_ThMp");

            entity.Property(e => e.IdBaiLamTongDiem).HasColumnName("idBaiLamTongDiem");
            entity.Property(e => e.IdThMp).HasColumnName("idThMp");
            entity.Property(e => e.ThoiDiemNguoiDungNhan).HasColumnName("thoiDiemNguoiDungNhan");

            entity.HasOne(d => d.IdBaiLamTongDiemNavigation).WithMany(p => p.DiemTungTinhHuongs)
                .HasForeignKey(d => d.IdBaiLamTongDiem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemTungTinhHuong_BaiLamMoPhong");

            entity.HasOne(d => d.IdThMpNavigation).WithMany(p => p.DiemTungTinhHuongs)
                .HasForeignKey(d => d.IdThMp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemTungTinhHuong_TinhHuongMoPhong");
        });

        modelBuilder.Entity<FlashCard>(entity =>
        {
            entity.HasKey(e => e.IdFlashcard).HasName("PK__FlashCar__06A1C19BCB2A6BD9");

            entity.ToTable("FlashCard");

            entity.HasIndex(e => e.IdBienBao, "IX_FlashCard_BienBao");

            entity.HasIndex(e => e.UserId, "IX_FlashCard_User");

            entity.Property(e => e.DanhGia).HasMaxLength(255);

            entity.HasOne(d => d.IdBienBaoNavigation).WithMany(p => p.FlashCards)
                .HasForeignKey(d => d.IdBienBao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FlashCard_BienBao");

            entity.HasOne(d => d.User).WithMany(p => p.FlashCards)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FlashCard_User");
        });

        modelBuilder.Entity<Hang>(entity =>
        {
            entity.HasKey(e => e.IdHang).HasName("PK__hang__03D9F6796D7E8455");

            entity.ToTable("hang");

            entity.Property(e => e.IdHang).HasColumnName("idHang");
            entity.Property(e => e.ChiPhi)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("chiPhi");
            entity.Property(e => e.DiemDat).HasColumnName("diemDat");
            entity.Property(e => e.GhiChu).HasColumnName("ghiChu");
            entity.Property(e => e.MaHang)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("maHang");
            entity.Property(e => e.MoTa).HasColumnName("moTa");
            entity.Property(e => e.SoCauHoi).HasColumnName("soCauHoi");
            entity.Property(e => e.SucKhoe)
                .HasMaxLength(255)
                .HasColumnName("sucKhoe");
            entity.Property(e => e.TaoLuc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("taoLuc");
            entity.Property(e => e.TenDayDu)
                .HasMaxLength(255)
                .HasColumnName("tenDayDu");
            entity.Property(e => e.ThoiGianTn).HasColumnName("thoiGianTn");
            entity.Property(e => e.TuoiToiDa).HasColumnName("tuoiToiDa");
            entity.Property(e => e.TuoiToiThieu).HasColumnName("tuoiToiThieu");
        });

        modelBuilder.Entity<HoSoThiSinh>(entity =>
        {
            entity.HasKey(e => e.HoSoId).HasName("PK__hoSoThiS__4D5BDEF2293E3537");

            entity.ToTable("hoSoThiSinh");

            entity.HasIndex(e => e.UserId, "IX_HoSoThiSinh_User");

            entity.Property(e => e.HoSoId).HasColumnName("hoSoId");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.KhamSucKhoe)
                .HasMaxLength(255)
                .HasColumnName("khamSucKhoe");
            entity.Property(e => e.LoaiHoSo)
                .HasMaxLength(100)
                .HasColumnName("loaiHoSo");
            entity.Property(e => e.NgayDk).HasColumnName("ngayDk");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.DaDuyet)
                .HasColumnName("daDuyet");


            entity.HasOne(d => d.User).WithMany(p => p.HoSoThiSinhs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoSoThiSinh_User");
        });

        modelBuilder.Entity<HoaDonThanhToan>(entity =>
        {
            entity.HasKey(e => e.IdThanhToan).HasName("PK__hoaDonTh__2DE12A6639EFC7F2");

            entity.ToTable("hoaDonThanhToan");

            entity.HasIndex(e => e.IdDangKy, "IX_HoaDonThanhToan_DangKy");
            entity.HasIndex(e => e.PhieuTxId, "IX_HoaDonThanhToan_PhieuTx");

            entity.Property(e => e.IdThanhToan).HasColumnName("idThanhToan");
            entity.Property(e => e.IdDangKy).HasColumnName("idDangKy");
            entity.Property(e => e.NgayThanhToan).HasColumnName("ngayThanhToan");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(100)
                .HasColumnName("phuongThucThanhToan");
            entity.Property(e => e.SoTien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("soTien");
            entity.Property(e => e.TrangThai).HasColumnName("trangThai");

            entity.HasOne(d => d.IdDangKyNavigation)
                .WithMany(p => p.HoaDonThanhToans)
                .HasForeignKey(d => d.IdDangKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoaDonThanhToan_DangKyHoc");

            entity.HasOne(d => d.PhieuTx)
                .WithMany(p => p.HoaDonThanhToans)
                .HasForeignKey(d => d.PhieuTxId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoaDonThanhToan_PhieuThueXe");
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.KqHocTapId).HasName("PK__KetQuaHo__B3EAB5AD95BEDCFF");

            entity.ToTable("KetQuaHocTap");

            entity.HasIndex(e => e.HoSoId, "IX_KetQuaHocTap_HoSo");

            entity.Property(e => e.DauTn).HasColumnName("Dau_TN");
            entity.Property(e => e.DuDkThiSh).HasColumnName("Du_Dk_ThiSH");
            entity.Property(e => e.DuDkThiTn).HasColumnName("Du_Dk_ThiTN");
            entity.Property(e => e.GioBanDem).HasColumnName("Gio_BanDem");
            entity.Property(e => e.HtDuongTruong).HasColumnName("Ht_DuongTruong");
            entity.Property(e => e.HtLyThuyet).HasColumnName("Ht_LyThuyet");
            entity.Property(e => e.HtMoPhong).HasColumnName("Ht_MoPhong");
            entity.Property(e => e.HtSaHinh).HasColumnName("Ht_SaHinh");
            entity.Property(e => e.ThoiGianCapNhat)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.HoSo).WithMany(p => p.KetQuaHocTaps)
                .HasForeignKey(d => d.HoSoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaHocTap_HoSoThiSinh");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.KhoaHocId).HasName("PK__khoaHoc__27F0D92B8F10EC99");

            entity.ToTable("khoaHoc");

            entity.Property(e => e.KhoaHocId).HasColumnName("khoaHocId");
            entity.Property(e => e.IdHang).HasColumnName("idHang");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.MoTa).HasColumnName("moTa");
            entity.Property(e => e.NgayBatDau).HasColumnName("ngayBatDau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayKetThuc");
            entity.Property(e => e.SlToiDa).HasColumnName("slToiDa");
            entity.Property(e => e.TenKhoaHoc)
                .HasMaxLength(255)
                .HasColumnName("tenKhoaHoc");

            entity.HasOne(d => d.IdHangNavigation).WithMany(p => p.KhoaHocs)
                .HasForeignKey(d => d.IdHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhoaHoc_Hang");
        });

        modelBuilder.Entity<LichHoc>(entity =>
        {
            entity.HasKey(e => e.LichHocId).HasName("PK__lichHoc__07EE5C71949F1DDD");

            entity.ToTable("lichHoc");

            entity.HasIndex(e => e.KhoaHocId, "IX_LichHoc_KhoaHoc");

            entity.HasIndex(e => e.LopHocId, "IX_LichHoc_LopHoc");

            entity.HasIndex(e => e.XeTapLaiId, "IX_LichHoc_XeTapLai");

            entity.Property(e => e.LichHocId).HasColumnName("lichHocId");
            entity.Property(e => e.DiaDiem)
                .HasMaxLength(255)
                .HasColumnName("diaDiem");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.KhoaHocId).HasColumnName("khoaHocId");
            entity.Property(e => e.LopHocId).HasColumnName("lopHocId");
            entity.Property(e => e.NgayHoc).HasColumnName("ngayHoc");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.TgBatDau).HasColumnName("tgBatDau");
            entity.Property(e => e.TgKetThuc).HasColumnName("tgKetThuc");
            entity.Property(e => e.XeTapLaiId).HasColumnName("xeTapLaiId");

            entity.HasOne(d => d.KhoaHoc).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.KhoaHocId)
                .HasConstraintName("FK_LichHoc_KhoaHoc");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_LichHoc_LopHoc");

            entity.HasOne(d => d.XeTapLai).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.XeTapLaiId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_LichHoc_XeTapLai");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.LopHocId).HasName("PK__lopHoc__331F37D994384D3E");

            entity.ToTable("lopHoc");

            entity.HasIndex(e => e.TenLop, "IX_LopHoc_TenLop");

            entity.Property(e => e.LopHocId).HasColumnName("lopHocId");
            entity.Property(e => e.TenLop)
                .HasMaxLength(255)
                .HasColumnName("tenLop");
            entity.Property(e => e.TrangThaiLop).HasColumnName("trangThaiLop");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__messages__4808B9936A63B357");

            entity.ToTable("messages");

            entity.HasIndex(e => e.ConversationsId, "IX_Messages_Conversation");

            entity.HasIndex(e => new { e.ConversationsId, e.SentAt }, "IX_Messages_Conversation_SentAt").IsDescending(false, true);

            entity.Property(e => e.MessageId).HasColumnName("messageId");
            entity.Property(e => e.ConversationsId).HasColumnName("conversationsId");
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.MessageText).HasColumnName("messageText");
            entity.Property(e => e.SentAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("sentAt");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Conversations).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Conversations");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_User");
        });

        modelBuilder.Entity<PhanHoi>(entity =>
        {
            entity.HasKey(e => e.PhanHoiId).HasName("PK__phanHoi__E9D3AD77C522D76C");

            entity.ToTable("phanHoi");

            entity.Property(e => e.PhanHoiId).HasColumnName("phanHoiId");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.ThoiGianPh)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoiGianPh");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.SoSao)
                .HasColumnName("soSao")
                .HasColumnType("decimal(2,1)")
                .HasDefaultValue(0);

            entity.HasOne(d => d.User).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhanHoi_User");
        });

        modelBuilder.Entity<QuyDinhHang>(entity =>
        {
            entity.HasKey(e => e.QuyDinhHangId).HasName("PK__quyDinhH__6B445BBB0AA70A45");

            entity.ToTable("quyDinhHang");

            entity.Property(e => e.QuyDinhHangId).HasColumnName("quyDinhHangId");
            entity.Property(e => e.DuongTruong).HasColumnName("duongTruong");
            entity.Property(e => e.GhiChu).HasColumnName("ghiChu");
            entity.Property(e => e.IdHang).HasColumnName("idHang");
            entity.Property(e => e.KmToiThieu).HasColumnName("kmToiThieu");
            entity.Property(e => e.LyThuyet).HasColumnName("lyThuyet");
            entity.Property(e => e.MoPhong).HasColumnName("moPhong");
            entity.Property(e => e.SaHinh).HasColumnName("saHinh");
            entity.Property(e => e.SoGioBanDem).HasColumnName("soGioBanDem");

            entity.HasOne(d => d.IdHangNavigation).WithMany(p => p.QuyDinhHangs)
                .HasForeignKey(d => d.IdHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuyDinhHang_Hang");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__role__CD98462A0A135486");

            entity.ToTable("role");

            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("roleName");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.ThongBaoId).HasName("PK__thongBao__C6F11B23A84AAB8E");

            entity.ToTable("thongBao");

            entity.Property(e => e.ThongBaoId).HasColumnName("thongBaoId");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.SendRole)
                .HasMaxLength(50)
                .HasColumnName("sendRole");
            entity.Property(e => e.TaoLuc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("taoLuc");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(255)
                .HasColumnName("tieuDe");
        });

        modelBuilder.Entity<TinhHuongMoPhong>(entity =>
        {
            entity.HasKey(e => e.IdThMp).HasName("PK__tinhHuon__BC9AF9FA124BA4D1");

            entity.ToTable("tinhHuongMoPhong");

            entity.HasIndex(e => e.IdChuongMp, "IX_TinhHuongMoPhong_ChuongMp");

            entity.Property(e => e.IdThMp).HasColumnName("idThMp");
            entity.Property(e => e.IdChuongMp).HasColumnName("idChuongMp");
            entity.Property(e => e.Kho).HasColumnName("kho");
            entity.Property(e => e.NgayTao)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TgBatDau).HasColumnName("tgBatDau");
            entity.Property(e => e.TgKetThuc).HasColumnName("tgKetThuc");
            entity.Property(e => e.ThuTu).HasColumnName("thuTu");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(255)
                .HasColumnName("tieuDe");
            entity.Property(e => e.UrlAnhMeo)
                .HasMaxLength(500)
                .HasColumnName("urlAnhMeo");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(1000)
                .HasColumnName("videoUrl");

            entity.HasOne(d => d.IdChuongMpNavigation).WithMany(p => p.TinhHuongMoPhongs)
                .HasForeignKey(d => d.IdChuongMp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TinhHuongMoPhong_ChuongMoPhong");
        });

        modelBuilder.Entity<TtGiaoVien>(entity =>
        {
            entity.HasKey(e => e.TtGiaoVienId).HasName("PK__ttGiaoVi__99DEABD62F83A653");

            entity.ToTable("ttGiaoVien");

            entity.Property(e => e.TtGiaoVienId).HasColumnName("ttGiaoVienId");
            entity.Property(e => e.ChuyenDaoTao)
                .HasMaxLength(255)
                .HasColumnName("chuyenDaoTao");
            entity.Property(e => e.ChuyenMon)
                .HasMaxLength(255)
                .HasColumnName("chuyenMon");
            entity.Property(e => e.NgayBatDauLam).HasColumnName("ngayBatDauLam");
            entity.Property(e => e.LichDay)
                .HasMaxLength(255)
                .HasColumnName("lichDay");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.TtGiaoViens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TtGiaoVien_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__user__CB9A1CFF4953EA8E");

            entity.ToTable("user");

            entity.HasIndex(e => e.RoleId, "IX_User_RoleId");

            entity.HasIndex(e => e.Username, "IX_User_Username");

            entity.HasIndex(e => e.Username, "UQ__user__F3DBC57264423334").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Avatar)
                .HasMaxLength(500)
                .HasColumnName("avatar");
            entity.Property(e => e.CapNhatLuc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("capNhatLuc");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("cccd");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(255)
                .HasColumnName("diaChi");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(10)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.LaGiaoVien).HasColumnName("laGiaoVien");
            entity.Property(e => e.LanDangNhapGanNhat)
                .HasPrecision(3)
                .HasColumnName("lanDangNhapGanNhat");
            entity.Property(e => e.NgaySinh).HasColumnName("ngaySinh");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .HasColumnName("soDienThoai");
            entity.Property(e => e.TaoLuc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("taoLuc");
            entity.Property(e => e.TenDayDu)
                .HasMaxLength(120)
                .HasColumnName("tenDayDu");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trangThai");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_User_Role");
        });

        modelBuilder.Entity<WebsocketConnection>(entity =>
        {
            entity.HasKey(e => e.ConnectionId).HasName("PK__websocke__A041D5C04CB38E95");

            entity.ToTable("websocketConnections");

            entity.HasIndex(e => e.UserId, "IX_WebsocketConnections_User");

            entity.Property(e => e.ConnectionId).HasColumnName("connectionId");
            entity.Property(e => e.ClientInfo)
                .HasMaxLength(400)
                .HasColumnName("clientInfo");
            entity.Property(e => e.ConnectedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("connectedAt");
            entity.Property(e => e.LastActivity)
                .HasPrecision(3)
                .HasColumnName("lastActivity");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.IsOnline)
                .HasColumnName("isOnline")
                .HasDefaultValue(false);

        entity.HasOne(d => d.User).WithMany(p => p.WebsocketConnections)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsocketConnections_User");
        });

        modelBuilder.Entity<XeTapLai>(entity =>
        {
            entity.HasKey(e => e.XeTapLaiId).HasName("PK__xeTapLai__E5BF79AC7C779A0A");

            entity.ToTable("xeTapLai");

            entity.HasIndex(e => e.LoaiXe, "IX_XeTapLai_LoaiXe");

            entity.Property(e => e.XeTapLaiId).HasColumnName("xeTapLaiId");
            entity.Property(e => e.LoaiXe)
                .HasMaxLength(50)
                .HasColumnName("loaiXe");
            entity.Property(e => e.TrangThaiXe).HasColumnName("trangThaiXe");
            entity.Property(e => e.BienSo)
                .HasMaxLength(20)
                .HasColumnName("bienSo");
            entity.Property(e => e.GiaThueTheoGio)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("giaThueTheoGio");
            entity.Property(e => e.AnhXe)
                .HasMaxLength(300)
                .HasColumnName("anhXe");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
