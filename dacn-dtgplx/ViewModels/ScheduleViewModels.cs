namespace dacn_dtgplx.ViewModels;

public class ScheduleItemVm
{
    public string Type { get; set; } = null!;
    // "KHOA_HOC" | "THUE_XE"

    public DateOnly Ngay { get; set; }

    public double GioBatDau { get; set; }   // ví dụ 3.5 = 3h30
    public double GioKetThuc { get; set; }

    public string TieuDe { get; set; } = null!;
    public string? NoiDung { get; set; }
    public string? DiaDiem { get; set; }

    public string Mau { get; set; } = "#4CAF50";

    // Thông tin hover
    public string HoverHtml { get; set; } = null!;
}

public class ScheduleWeekVm
{
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }

    public bool IsCurrentWeek { get; set; }

    public int TotalSessions { get; set; }   // 🔢 số buổi học / thuê xe

    public List<ScheduleItemVm> Items { get; set; } = new();
}
