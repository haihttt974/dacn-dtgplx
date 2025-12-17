namespace dacn_dtgplx.ViewModels.Reports
{
    public class RevenueMonthRowVM
    {
        public string MonthLabel { get; set; } = "";      // "5/2025"
        public decimal RevenueTotal { get; set; }
        public decimal RevenueCourses { get; set; }
        public decimal RevenueVehicles { get; set; }
        public int Transactions { get; set; }
    }
}
