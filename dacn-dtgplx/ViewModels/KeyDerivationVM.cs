namespace dacn_dtgplx.ViewModels
{
    public class KeyDerivationVM
    {
        public string Function { get; set; } = "PBKDF2";
        public string Hash { get; set; } = "HMACSHA256";
        public int Iterations { get; set; }
        public int SaltSizeBytes { get; set; }
    }
}
