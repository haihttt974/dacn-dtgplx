CREATE DATABASE DT_GPLX
GO
USE DT_GPLX
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- =====================================================
-- TẠO BẢNG
-- =====================================================

/* ==================== PHAN QUYEN & NGUOI DUNG ==================== */
CREATE TABLE role (
    roleId INT IDENTITY(1,1) PRIMARY KEY,
    roleName NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL
);

CREATE TABLE [user] (
    userId INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(100) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    tenDayDu NVARCHAR(120) NULL,
    email NVARCHAR(255) NULL,
    soDienThoai NVARCHAR(20) NULL,
    diaChi NVARCHAR(255) NULL,
    cccd NVARCHAR(20) NULL,
    gioiTinh NVARCHAR(10) NULL,
    ngaySinh DATE NULL,
    laGiaoVien BIT NOT NULL DEFAULT 0,
    trangThai BIT NOT NULL DEFAULT 1,
    avatar NVARCHAR(500) NULL,
    lanDangNhapGanNhat DATETIME2(3) NULL,
    roleId INT NULL,
    taoLuc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    capNhatLuc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
);

CREATE TABLE ttGiaoVien (
    ttGiaoVienId INT IDENTITY(1,1) PRIMARY KEY,
    chuyenMon NVARCHAR(255),
    chuyenDaoTao NVARCHAR(255),
    ngayBatDauLam DATE,
    userId INT NOT NULL
);

CREATE TABLE phanHoi (
    phanHoiId INT IDENTITY(1,1) PRIMARY KEY,
    noiDung NVARCHAR(MAX) NOT NULL,
    thoiGianPh DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    userId INT NOT NULL
);
GO
ALTER TABLE phanHoi
ADD soSao DECIMAL(2,1) NOT NULL;
GO
ALTER TABLE phanHoi
ADD CONSTRAINT CK_PhanHoi_SoSao CHECK (soSao IS NULL OR (soSao >= 0 AND soSao <= 5));
GO

/* ==================== NHAN TIN & REALTIME ==================== */
CREATE TABLE conversations (
    conversationsId INT IDENTITY(1,1) PRIMARY KEY,
    userId INT NOT NULL,
    userId2 INT NOT NULL,
    createdAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    lastMessageAt DATETIME2(3) NULL
);

CREATE TABLE messages (
    messageId INT IDENTITY(1,1) PRIMARY KEY,
    conversationsId INT NOT NULL,
    userId INT NOT NULL,
    messageText NVARCHAR(MAX) NOT NULL,
    sentAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    isRead BIT NOT NULL DEFAULT 0
);

CREATE TABLE websocketConnections (
    connectionId BIGINT IDENTITY(1,1) PRIMARY KEY,
    connectedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    lastActivity DATETIME2(3) NULL,
    clientInfo NVARCHAR(400) NULL,
	isOnline BIT,
    userId INT NOT NULL
);

/* =============== FLASH CARD & BIỂN BÁO =============== */
CREATE TABLE dbo.BienBao (
    IdBienBao   INT IDENTITY(1,1) PRIMARY KEY,
    TenBienBao  NVARCHAR(255) NOT NULL,
    YNghia      NVARCHAR(MAX) NULL,
    HinhAnh     NVARCHAR(500) NULL
);

CREATE TABLE dbo.FlashCard (
    IdFlashcard INT IDENTITY(1,1) PRIMARY KEY,
    DanhGia     NVARCHAR(255) NULL,
    UserId      INT NOT NULL,     -- FK (phần 2)
    IdBienBao   INT NOT NULL      -- FK (phần 2)
);

/* ==================== THÔNG BÁO ==================== */
CREATE TABLE thongBao (
    thongBaoId INT IDENTITY(1,1) PRIMARY KEY,
    tieuDe NVARCHAR(255) NOT NULL,
    noiDung NVARCHAR(MAX) NOT NULL,
    taoLuc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    sendRole NVARCHAR(50) NULL  -- đối tượng được gửi: ALL / HOCVIEN / GIAOVIEN
);

CREATE TABLE ctThongBao (
    userId INT NOT NULL,
    thongBaoId INT NOT NULL,
    thoiGianGui DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    daXem BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (userId, thongBaoId)
);

/* ==================== HO SO – DANG KY – THANH TOAN ==================== */
CREATE TABLE hoSoThiSinh (
    hoSoId INT IDENTITY(1,1) PRIMARY KEY,
    loaiHoSo NVARCHAR(100),
    ngayDk DATE,
    khamSucKhoe NVARCHAR(255),
    ghiChu NVARCHAR(255),
	daDuyet BIT,
    userId INT NOT NULL
);

GO
ALTER TABLE hoSoThiSinh
ALTER COLUMN daDuyet BIT NULL;
GO

CREATE TABLE dbo.KetQuaHocTap (
    KqHocTapId      INT IDENTITY(1,1) PRIMARY KEY,
    NhanXet         NVARCHAR(MAX) NULL,
    SoBuoiDaHoc     INT NULL,
    SoBuoiToiThieu  INT NULL,
    KmHoanThanh     INT NULL,
    Gio_BanDem      INT NULL,
    Ht_LyThuyet     BIT NULL,
    Ht_MoPhong      BIT NULL,
    Ht_SaHinh       BIT NULL,
    Ht_DuongTruong  BIT NULL,
    Du_Dk_ThiTN     BIT NULL,
    Dau_TN          BIT NULL,
    Du_Dk_ThiSH     BIT NULL,
    ThoiGianCapNhat DATETIME2(3) NOT NULL CONSTRAINT DF_KQHT_CapNhat DEFAULT (SYSUTCDATETIME()),
    HoSoId          INT NOT NULL -- FK (phần 2)
);

CREATE TABLE dangKyHoc (
    idDangKy INT IDENTITY(1,1) PRIMARY KEY,
    ngayDangKy DATE,
    trangThai BIT,
    ghiChu NVARCHAR(255),
    hoSoId INT NOT NULL,
    khoaHocId INT NOT NULL
);

CREATE TABLE hoaDonThanhToan (
    idThanhToan INT IDENTITY(1,1) PRIMARY KEY,
    ngayThanhToan DATE,
    soTien DECIMAL(18,2),
    phuongThucThanhToan NVARCHAR(100),
    trangThai BIT,
    noiDung NVARCHAR(MAX),
    idDangKy INT NOT NULL
);

/* ==================== HANG GPLX – KHOA HOC – QUY DINH ==================== */
CREATE TABLE hang (
    idHang INT IDENTITY(1,1) PRIMARY KEY,
    maHang NCHAR(10) NOT NULL,
    tenDayDu NVARCHAR(255),
    moTa NVARCHAR(MAX),
    diemDat INT NOT NULL DEFAULT 0,
    thoiGianTn INT NOT NULL,
    soCauHoi INT NOT NULL DEFAULT 0,
	taoLuc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    tuoiToiThieu INT NULL,
    tuoiToiDa INT NULL,
    sucKhoe NVARCHAR(255) NULL,
	chiPhi DECIMAL(18,2),
    ghiChu NVARCHAR(MAX) NULL
);

CREATE TABLE quyDinhHang (
    quyDinhHangId INT IDENTITY(1,1) PRIMARY KEY,
    kmToiThieu INT,
    soGioBanDem INT,
    lyThuyet BIT,
    saHinh BIT,
    moPhong BIT,
    duongTruong BIT,
    ghiChu NVARCHAR(MAX),
    idHang INT NOT NULL
);

CREATE TABLE khoaHoc (
    khoaHocId INT IDENTITY(1,1) PRIMARY KEY,
    tenKhoaHoc NVARCHAR(255),
    ngayBatDau DATE,
    ngayKetThuc DATE,
    slToiDa INT,  -- Max students
    moTa NVARCHAR(MAX),
    isActive BIT DEFAULT 1, -- đóng / mở khóa học
    idHang INT NOT NULL
);

/* ==================== LICH HOC – LOP HOC – XE TAP LAI ==================== */

CREATE TABLE lopHoc (
    lopHocId INT IDENTITY(1,1) PRIMARY KEY,
    tenLop NVARCHAR(255) NOT NULL,
    trangThaiLop BIT NOT NULL
);

CREATE TABLE xeTapLai (
    xeTapLaiId INT IDENTITY(1,1) PRIMARY KEY,
    loaiXe NVARCHAR(50) NOT NULL,
    trangThaiXe BIT NOT NULL
);
GO
ALTER TABLE xeTapLai
ADD bienSo NVARCHAR(20) NOT NULL,
    giaThueTheoGio DECIMAL(18,2) NOT NULL;
GO
CREATE TABLE lichHoc (
    lichHocId INT IDENTITY(1,1) PRIMARY KEY,
    xeTapLaiId INT NULL,     -- FK đến xe tập lái (nếu là buổi thực hành)
    lopHocId INT NULL,       -- FK đến lớp học (nếu là buổi lý thuyết hoặc mô phỏng)
    khoaHocId INT NOT NULL,  -- FK đến khóa học
    
    ngayHoc DATE NOT NULL,
    tgBatDau TIME NOT NULL,
    tgKetThuc TIME NOT NULL,
    noiDung NVARCHAR(MAX),
    diaDiem NVARCHAR(255),
    ghiChu NVARCHAR(255),

    CONSTRAINT CK_LichHoc_XeOrLop CHECK (
        (xeTapLaiId IS NOT NULL AND lopHocId IS NULL) OR
        (xeTapLaiId IS NULL AND lopHocId IS NOT NULL)
    )
);
GO

/* ==================== LY THUYET ==================== */
CREATE TABLE chuong (
    chuongId INT IDENTITY(1,1) PRIMARY KEY,
    tenChuong NVARCHAR(255),
    thuTu INT
);

CREATE TABLE cauHoiLyThuyet (
    idCauHoi INT IDENTITY(1,1) PRIMARY KEY,
    chuongId INT NOT NULL,
    noiDung NVARCHAR(MAX),
    hinhAnh NVARCHAR(500),
    cauLiet BIT,
    chuY BIT ,
    xeMay BIT,
    urlAnhMeo NVARCHAR(500)
);

CREATE TABLE dapAn (
    idDapAn INT IDENTITY(1,1) PRIMARY KEY,
    idCauHoi INT NOT NULL,
    noiDung NVARCHAR(MAX),
    dapAnDung BIT NOT NULL DEFAULT 0,
    thuTu INT NOT NULL DEFAULT 0
);

/* ==================== THI THU LY THUYET ==================== */
CREATE TABLE boDeThiThu (
    idBoDe INT IDENTITY(1,1) PRIMARY KEY,
    tenBoDe NVARCHAR(255),
    thoiGian INT,
    soCauHoi INT,
    hoatDong BIT DEFAULT 1,
    taoLuc DATETIME2(3) DEFAULT SYSUTCDATETIME(),
    idHang INT NOT NULL
);

CREATE TABLE chiTietBoDeTN (
    idBoDe INT NOT NULL,
    idCauHoi INT NOT NULL,
    thuTu INT DEFAULT 0,
    PRIMARY KEY(idBoDe, idCauHoi)
);

CREATE TABLE baiLam (
    baiLamId INT IDENTITY(1,1) PRIMARY KEY,
    thoiGianLamBai INT,
    soCauSai INT DEFAULT 0,
    ketQua BIT,
    userId INT NOT NULL,
    idBoDe INT NOT NULL
);

CREATE TABLE chiTietBaiLam (
    baiLamId INT NOT NULL,
    idCauHoi INT NOT NULL,
    dapAnDaChon NVARCHAR(50),
    ketQuaCau BIT,
    PRIMARY KEY(baiLamId, idCauHoi)
);

/* ==================== MO PHONG ==================== */
CREATE TABLE chuongMoPhong (
    idChuongMp INT IDENTITY(1,1) PRIMARY KEY,
    tenChuong NVARCHAR(255),
    thuTu INT
);

CREATE TABLE tinhHuongMoPhong (
    idThMp INT IDENTITY(1,1) PRIMARY KEY,
    idChuongMp INT NOT NULL,
    tieuDe NVARCHAR(255),
    videoUrl NVARCHAR(1000),
    thuTu INT,
    kho BIT,
    tgBatDau FLOAT,
    tgKetThuc FLOAT,
	ngayTao DATETIME2(3) DEFAULT SYSUTCDATETIME(),
    urlAnhMeo NVARCHAR(500)
);

CREATE TABLE boDeMoPhong (
    idBoDeMoPhong INT IDENTITY(1,1) PRIMARY KEY,
    tenBoDe NVARCHAR(255),
    soTinhHuong INT,
    taoLuc DATETIME2(3) DEFAULT SYSUTCDATETIME(),
    isActive BIT DEFAULT 1
);

CREATE TABLE chiTietBoDeMoPhong (
    idBoDeMoPhong INT NOT NULL,
    idThMp INT NOT NULL,
    thuTu INT,
    PRIMARY KEY(idBoDeMoPhong, idThMp)
);

CREATE TABLE baiLamMoPhong (
    idBaiLamTongDiem INT IDENTITY(1,1) PRIMARY KEY,
    tongDiem INT DEFAULT 0,
    ketQua BIT,
    userId INT NOT NULL,
    idBoDeMoPhong INT NOT NULL
);

CREATE TABLE diemTungTinhHuong (
    idBaiLamTongDiem INT NOT NULL,
    idThMp INT NOT NULL,
    thoiDiemNguoiDungNhan FLOAT NOT NULL,
    PRIMARY KEY(idBaiLamTongDiem, idThMp)
);
GO

-- =====================================================
-- MỐI KẾT HỢP VÀ KHÓA NGOẠI
-- =====================================================

/* ==================== USER & ROLE ==================== */
ALTER TABLE [user]
ADD CONSTRAINT FK_User_Role
FOREIGN KEY (roleId) REFERENCES role(roleId);

ALTER TABLE ttGiaoVien
ADD CONSTRAINT FK_TtGiaoVien_User
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE phanHoi
ADD CONSTRAINT FK_PhanHoi_User
FOREIGN KEY (userId) REFERENCES [user](userId);


/* ==================== NHẮN TIN ==================== */
ALTER TABLE conversations
ADD CONSTRAINT FK_Conversations_User1
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE conversations
ADD CONSTRAINT FK_Conversations_User2
FOREIGN KEY (userId2) REFERENCES [user](userId);

ALTER TABLE messages
ADD CONSTRAINT FK_Messages_Conversations
FOREIGN KEY (conversationsId) REFERENCES conversations(conversationsId);

ALTER TABLE messages
ADD CONSTRAINT FK_Messages_User
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE websocketConnections
ADD CONSTRAINT FK_WebsocketConnections_User
FOREIGN KEY (userId) REFERENCES [user](userId);


/* ==================== FLASH CARD & BIỂN BÁO ==================== */
ALTER TABLE FlashCard
ADD CONSTRAINT FK_FlashCard_User
FOREIGN KEY (UserId) REFERENCES [user](userId);

ALTER TABLE FlashCard
ADD CONSTRAINT FK_FlashCard_BienBao
FOREIGN KEY (IdBienBao) REFERENCES BienBao(IdBienBao);

/* ==================== THÔNG BÁO ==================== */
ALTER TABLE ctThongBao
ADD CONSTRAINT FK_CtThongBao_User
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE ctThongBao
ADD CONSTRAINT FK_CtThongBao_ThongBao
FOREIGN KEY (thongBaoId) REFERENCES thongBao(thongBaoId);

/* ==================== HỒ SƠ – ĐĂNG KÝ – HỌC TẬP ==================== */
ALTER TABLE hoSoThiSinh
ADD CONSTRAINT FK_HoSoThiSinh_User
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE KetQuaHocTap
ADD CONSTRAINT FK_KetQuaHocTap_HoSoThiSinh
FOREIGN KEY (HoSoId) REFERENCES hoSoThiSinh(hoSoId);

ALTER TABLE dangKyHoc
ADD CONSTRAINT FK_DangKyHoc_HoSoThiSinh
FOREIGN KEY (hoSoId) REFERENCES hoSoThiSinh(hoSoId);

ALTER TABLE dangKyHoc
ADD CONSTRAINT FK_DangKyHoc_KhoaHoc
FOREIGN KEY (khoaHocId) REFERENCES khoaHoc(khoaHocId);

ALTER TABLE hoaDonThanhToan
ADD CONSTRAINT FK_HoaDonThanhToan_DangKyHoc
FOREIGN KEY (idDangKy) REFERENCES dangKyHoc(idDangKy);


/* ==================== HẠNG – KHOÁ HỌC – QUY ĐỊNH ==================== */
ALTER TABLE quyDinhHang
ADD CONSTRAINT FK_QuyDinhHang_Hang
FOREIGN KEY (idHang) REFERENCES hang(idHang);

ALTER TABLE khoaHoc
ADD CONSTRAINT FK_KhoaHoc_Hang
FOREIGN KEY (idHang) REFERENCES hang(idHang);


/* ==================== LỊCH HỌC – LỚP HỌC – XE TẬP LÁI ==================== */
ALTER TABLE lichHoc
ADD CONSTRAINT FK_LichHoc_KhoaHoc
FOREIGN KEY (khoaHocId) REFERENCES khoaHoc(khoaHocId) ON DELETE CASCADE;

ALTER TABLE lichHoc
ADD CONSTRAINT FK_LichHoc_LopHoc
FOREIGN KEY (lopHocId) REFERENCES lopHoc(lopHocId) ON DELETE SET NULL;

ALTER TABLE lichHoc
ADD CONSTRAINT FK_LichHoc_XeTapLai
FOREIGN KEY (xeTapLaiId) REFERENCES xeTapLai(xeTapLaiId) ON DELETE SET NULL;
GO

/* ==================== LÝ THUYẾT ==================== */
ALTER TABLE cauHoiLyThuyet
ADD CONSTRAINT FK_CauHoiLyThuyet_Chuong
FOREIGN KEY (chuongId) REFERENCES chuong(chuongId);

ALTER TABLE dapAn
ADD CONSTRAINT FK_DapAn_CauHoiLyThuyet
FOREIGN KEY (idCauHoi) REFERENCES cauHoiLyThuyet(idCauHoi);


/* ==================== THI THỬ LÝ THUYẾT ==================== */
ALTER TABLE boDeThiThu
ADD CONSTRAINT FK_BoDeThiThu_Hang
FOREIGN KEY (idHang) REFERENCES hang(idHang);

ALTER TABLE chiTietBoDeTN
ADD CONSTRAINT FK_ChiTietBoDeTN_BoDeThiThu
FOREIGN KEY (idBoDe) REFERENCES boDeThiThu(idBoDe);

ALTER TABLE chiTietBoDeTN
ADD CONSTRAINT FK_ChiTietBoDeTN_CauHoiLyThuyet
FOREIGN KEY (idCauHoi) REFERENCES cauHoiLyThuyet(idCauHoi);

ALTER TABLE baiLam
ADD CONSTRAINT FK_BaiLam_User
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE baiLam
ADD CONSTRAINT FK_BaiLam_BoDeThiThu
FOREIGN KEY (idBoDe) REFERENCES boDeThiThu(idBoDe);

ALTER TABLE chiTietBaiLam
ADD CONSTRAINT FK_ChiTietBaiLam_BaiLam
FOREIGN KEY (baiLamId) REFERENCES baiLam(baiLamId);

ALTER TABLE chiTietBaiLam
ADD CONSTRAINT FK_ChiTietBaiLam_CauHoiLyThuyet
FOREIGN KEY (idCauHoi) REFERENCES cauHoiLyThuyet(idCauHoi);


/* ==================== MÔ PHỎNG ==================== */
ALTER TABLE tinhHuongMoPhong
ADD CONSTRAINT FK_TinhHuongMoPhong_ChuongMoPhong
FOREIGN KEY (idChuongMp) REFERENCES chuongMoPhong(idChuongMp);

ALTER TABLE chiTietBoDeMoPhong
ADD CONSTRAINT FK_CTBoDeMoPhong_BoDeMoPhong
FOREIGN KEY (idBoDeMoPhong) REFERENCES boDeMoPhong(idBoDeMoPhong);

ALTER TABLE chiTietBoDeMoPhong
ADD CONSTRAINT FK_CTBoDeMoPhong_TinhHuongMoPhong
FOREIGN KEY (idThMp) REFERENCES tinhHuongMoPhong(idThMp);

ALTER TABLE baiLamMoPhong
ADD CONSTRAINT FK_BaiLamMoPhong_User
FOREIGN KEY (userId) REFERENCES [user](userId);

ALTER TABLE baiLamMoPhong
ADD CONSTRAINT FK_BaiLamMoPhong_BoDeMoPhong
FOREIGN KEY (idBoDeMoPhong) REFERENCES boDeMoPhong(idBoDeMoPhong);

ALTER TABLE diemTungTinhHuong
ADD CONSTRAINT FK_DiemTungTinhHuong_BaiLamMoPhong
FOREIGN KEY (idBaiLamTongDiem) REFERENCES baiLamMoPhong(idBaiLamTongDiem);

ALTER TABLE diemTungTinhHuong
ADD CONSTRAINT FK_DiemTungTinhHuong_TinhHuongMoPhong
FOREIGN KEY (idThMp) REFERENCES tinhHuongMoPhong(idThMp);
GO


-- =====================================================
-- TẠO INDEX
-- =====================================================
/* ==================== USER ==================== */
-- Tìm theo username (đăng nhập)
CREATE INDEX IX_User_Username ON [user](username);

-- Tối ưu join theo role
CREATE INDEX IX_User_RoleId ON [user](roleId);


/* ==================== THÔNG BÁO ==================== */
-- User nhận thông báo, xem/ chưa xem
CREATE INDEX IX_CtThongBao_User ON ctThongBao(userId);
CREATE INDEX IX_CtThongBao_ThongBao ON ctThongBao(thongBaoId);


/* ==================== NHẮN TIN & REALTIME ==================== */
-- Duyệt tin nhắn theo hội thoại
CREATE INDEX IX_Messages_Conversation ON messages(conversationsId);

-- Tìm tin mới nhất của mỗi hội thoại
CREATE INDEX IX_Messages_Conversation_SentAt ON messages(conversationsId, sentAt DESC);

-- Tối ưu truy vấn thông tin user đang online
CREATE INDEX IX_WebsocketConnections_User ON websocketConnections(userId);


/* ==================== FLASH CARD ==================== */
CREATE INDEX IX_FlashCard_User ON FlashCard(UserId);
CREATE INDEX IX_FlashCard_BienBao ON FlashCard(IdBienBao);


/* ==================== HỒ SƠ – ĐĂNG KÝ – HỌC TẬP ==================== */
CREATE INDEX IX_HoSoThiSinh_User ON hoSoThiSinh(userId);

CREATE INDEX IX_KetQuaHocTap_HoSo ON KetQuaHocTap(HoSoId);

CREATE INDEX IX_DangKyHoc_HoSo ON dangKyHoc(hoSoId);
CREATE INDEX IX_DangKyHoc_KhoaHoc ON dangKyHoc(khoaHocId);

CREATE INDEX IX_HoaDonThanhToan_DangKy ON hoaDonThanhToan(idDangKy);


/* ==================== HẠNG – KHÓA – LỊCH – LỚP – XE ==================== */
CREATE INDEX IX_LichHoc_KhoaHoc ON lichHoc(khoaHocId);
CREATE INDEX IX_LichHoc_LopHoc ON lichHoc(lopHocId);
CREATE INDEX IX_LichHoc_XeTapLai ON lichHoc(xeTapLaiId);
CREATE INDEX IX_LopHoc_TenLop ON lopHoc(tenLop);
CREATE INDEX IX_XeTapLai_LoaiXe ON xeTapLai(loaiXe);
GO

/* ==================== LÝ THUYẾT ==================== */
-- Tối ưu tìm câu hỏi theo chương
CREATE INDEX IX_CauHoiLyThuyet_Chuong ON cauHoiLyThuyet(chuongId);

-- Tối ưu lấy đáp án theo câu hỏi
CREATE INDEX IX_DapAn_CauHoi ON dapAn(idCauHoi);


/* ==================== THI THỬ LÝ THUYẾT ==================== */
CREATE INDEX IX_BoDeThiThu_Hang ON boDeThiThu(idHang);

CREATE INDEX IX_ChiTietBoDeTN_BoDe ON chiTietBoDeTN(idBoDe);
CREATE INDEX IX_ChiTietBoDeTN_CauHoi ON chiTietBoDeTN(idCauHoi);

CREATE INDEX IX_BaiLam_User ON baiLam(userId);
CREATE INDEX IX_BaiLam_BoDe ON baiLam(idBoDe);

CREATE INDEX IX_ChiTietBaiLam_BaiLam ON chiTietBaiLam(baiLamId);


/* ==================== MÔ PHỎNG ==================== */
CREATE INDEX IX_TinhHuongMoPhong_ChuongMp ON tinhHuongMoPhong(idChuongMp);

CREATE INDEX IX_ChiTietBoDeMoPhong_BoDe ON chiTietBoDeMoPhong(idBoDeMoPhong);
CREATE INDEX IX_ChiTietBoDeMoPhong_ThMp ON chiTietBoDeMoPhong(idThMp);

CREATE INDEX IX_BaiLamMoPhong_User ON baiLamMoPhong(userId);
CREATE INDEX IX_BaiLamMoPhong_BoDeMp ON baiLamMoPhong(idBoDeMoPhong);

CREATE INDEX IX_DiemTungTinhHuong_BaiLamMp ON diemTungTinhHuong(idBaiLamTongDiem);
CREATE INDEX IX_DiemTungTinhHuong_ThMp ON diemTungTinhHuong(idThMp);
GO

ALTER TABLE ttGiaoVien
ADD lichDay NVARCHAR(255);
GO

CREATE TABLE PhieuThueXe (
    phieuTxId INT IDENTITY(1,1) PRIMARY KEY,
    userId INT NOT NULL,
    xeTapLaiId INT NOT NULL,
    tg_BatDau DATETIME,
    tg_Thue INT,

    -- Khóa ngoại
    CONSTRAINT FK_PhieuThueXe_User
        FOREIGN KEY (userId) REFERENCES [user](userId),

    CONSTRAINT FK_PhieuThueXe_Xe
        FOREIGN KEY (xeTapLaiId) REFERENCES xeTapLai(xeTapLaiId)
);
GO

ALTER TABLE hoaDonThanhToan
ALTER COLUMN idDangKy INT NULL;
GO

ALTER TABLE hoaDonThanhToan
ADD phieuTxId INT NULL;
GO

ALTER TABLE hoaDonThanhToan
ADD CONSTRAINT FK_HoaDon_PhieuThueXe
    FOREIGN KEY (phieuTxId) REFERENCES PhieuThueXe(phieuTxId);
GO