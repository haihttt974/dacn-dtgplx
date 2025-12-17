namespace dacn_dtgplx.ViewModels.Reports
{
    public class TestMonthRowVM
    {
        public string MonthLabel { get; set; } = "";

        // Attempts
        public int TheoryAttempts { get; set; }
        public int SimulationAttempts { get; set; }

        // Distinct users
        public int TheoryUsers { get; set; }
        public int SimulationUsers { get; set; }
    }
}
