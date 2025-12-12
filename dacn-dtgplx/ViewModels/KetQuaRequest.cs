namespace dacn_dtgplx.ViewModels
{
    public class KetQuaRequest
    {
        public int IdBoDe { get; set; }
        //public int UserId { get; set; }

        // danh sách 10 flag user nhấn trong bài thi
        public List<FlagItem> Flags { get; set; } = new();
    }
}
