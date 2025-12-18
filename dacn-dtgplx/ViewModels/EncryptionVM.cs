namespace dacn_dtgplx.ViewModels
{
    public class EncryptionVM
    {
        public int KeySizeBits { get; set; }
        public int IvSizeBytes { get; set; }
        public int AuthTagSizeBytes { get; set; }
    }
}
