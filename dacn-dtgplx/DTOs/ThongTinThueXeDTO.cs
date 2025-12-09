namespace dacn_dtgplx.DTOs
{
    public class ThongTinThueXeDTO
    {
        public string Ten { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string CCCD { get; set; }

        public int XeId { get; set; }
        public DateTime RentStart { get; set; }
        public int Duration { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
