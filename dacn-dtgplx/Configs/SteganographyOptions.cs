namespace dacn_dtgplx.Configs
{
    public class SteganographyOptions
    {
        public string EncryptionKey { get; set; } = string.Empty; // 32 ký tự
        public string Iv { get; set; } = string.Empty;            // 16 ký tự
    }
}
