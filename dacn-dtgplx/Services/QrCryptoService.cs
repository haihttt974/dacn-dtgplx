using dacn_dtgplx.ViewModels;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace dacn_dtgplx.Services
{
    public class QrCryptoService
    {
        private readonly CryptoSettingsVM _cfg;

        public QrCryptoService(IOptions<CryptoSettingsVM> options)
        {
            _cfg = options.Value;
        }

        // =========================
        // BASE64 URL SAFE
        // =========================
        private static string ToBase64Url(byte[] data)
        {
            return Convert.ToBase64String(data)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url
                .Replace("-", "+")
                .Replace("_", "/");

            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            return Convert.FromBase64String(padded);
        }

        // =========================
        // ENCRYPT (AES-256-GCM)
        // =========================
        public string Encrypt(string plainText)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_cfg.KeyDerivation.SaltSizeBytes);
            byte[] iv = RandomNumberGenerator.GetBytes(_cfg.Encryption.IvSizeBytes);

            byte[] key = DeriveKey(salt);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipher = new byte[plainBytes.Length];
            byte[] tag = new byte[_cfg.Encryption.AuthTagSizeBytes];

            using var aes = new AesGcm(key);
            aes.Encrypt(iv, plainBytes, cipher, tag);

            // Salt | IV | Cipher | Tag
            var result = new byte[
                salt.Length +
                iv.Length +
                cipher.Length +
                tag.Length
            ];

            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
            Buffer.BlockCopy(cipher, 0, result, salt.Length + iv.Length, cipher.Length);
            Buffer.BlockCopy(tag, 0, result, salt.Length + iv.Length + cipher.Length, tag.Length);

            return ToBase64Url(result);
        }

        // =========================
        // DECRYPT (AES-256-GCM)
        // =========================
        public string Decrypt(string base64Url)
        {
            byte[] data = FromBase64Url(base64Url);

            int saltLen = _cfg.KeyDerivation.SaltSizeBytes;
            int ivLen = _cfg.Encryption.IvSizeBytes;
            int tagLen = _cfg.Encryption.AuthTagSizeBytes;

            byte[] salt = data[..saltLen];
            byte[] iv = data[saltLen..(saltLen + ivLen)];
            byte[] tag = data[^tagLen..];
            byte[] cipher = data[(saltLen + ivLen)..^tagLen];

            byte[] key = DeriveKey(salt);
            byte[] plain = new byte[cipher.Length];

            using var aes = new AesGcm(key);
            aes.Decrypt(iv, cipher, tag, plain);

            return Encoding.UTF8.GetString(plain);
        }

        // =========================
        // PBKDF2 - HMACSHA256
        // =========================
        private byte[] DeriveKey(byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                _cfg.MasterKey,
                salt,
                _cfg.KeyDerivation.Iterations,
                HashAlgorithmName.SHA256
            );

            return pbkdf2.GetBytes(_cfg.Encryption.KeySizeBits / 8);
        }
    }
}
