using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace dacn_dtgplx.ViewModels
{
    public class CryptoSettingsVM
    {
        public string Algorithm { get; set; } = default!;
        public KeyDerivationVM KeyDerivation { get; set; } = default!;
        public EncryptionVM Encryption { get; set; } = default!;
        public string Encoding { get; set; } = "Base64";
        public string MasterKey { get; set; } = default!;
    }

}
