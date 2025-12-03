using System;
using System.Collections.Generic;

namespace dacn_dtgplx.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? TenDayDu { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public string? Cccd { get; set; }

    public string? GioiTinh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public bool LaGiaoVien { get; set; }

    public bool TrangThai { get; set; }

    public string? Avatar { get; set; }

    public DateTime? LanDangNhapGanNhat { get; set; }

    public int? RoleId { get; set; }

    public DateTime TaoLuc { get; set; }

    public DateTime CapNhatLuc { get; set; }

    public virtual ICollection<BaiLamMoPhong> BaiLamMoPhongs { get; set; } = new List<BaiLamMoPhong>();

    public virtual ICollection<BaiLam> BaiLams { get; set; } = new List<BaiLam>();

    public virtual ICollection<Conversation> ConversationUserId2Navigations { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationUsers { get; set; } = new List<Conversation>();

    public virtual ICollection<CtThongBao> CtThongBaos { get; set; } = new List<CtThongBao>();

    public virtual ICollection<FlashCard> FlashCards { get; set; } = new List<FlashCard>();

    public virtual ICollection<HoSoThiSinh> HoSoThiSinhs { get; set; } = new List<HoSoThiSinh>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TtGiaoVien> TtGiaoViens { get; set; } = new List<TtGiaoVien>();

    public virtual ICollection<WebsocketConnection> WebsocketConnections { get; set; } = new List<WebsocketConnection>();
}
