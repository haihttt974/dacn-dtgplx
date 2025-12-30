namespace dacn_dtgplx.ViewModels
{
    public class FeatureItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public List<int> Roles { get; set; } // role được phép thấy
    }
}
