namespace dacn_dtgplx.ViewModels.Reports
{
    public class UserMonthRowVM
    {
        public string MonthLabel { get; set; } = "";
        public int NewUsers { get; set; }                      // distinct user
        public int NewProfiles { get; set; }                   // hồ sơ
    }
}
