namespace dacn_dtgplx.DTOs
{
    public class ChartImageDto
    {
        public string Name { get; set; }
        public string ImageBase64 { get; set; }
        public string? Note { get; internal set; }
    }
}
