using dacn_dtgplx.Configs;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Security.Cryptography;
using System.Text;

namespace dacn_dtgplx.Services
{
    public class SteganographyService : ISteganographyService
    {
        private readonly IWebHostEnvironment _env;
        private readonly SteganographyOptions _opt;

        public SteganographyService(
            IWebHostEnvironment env,
            IOptions<SteganographyOptions> opt)
        {
            _env = env;
            _opt = opt.Value;
        }

        // ==================================================
        // 1)  GIẤU JSON
        // ==================================================
        public async Task<string> HideJsonIntoImageAsync(
    IFormFile uploadImage,
    string saveRelativePath,
    string json)
        {
            string physicalPath = Path.Combine(_env.WebRootPath, saveRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(uploadImage.OpenReadStream());

            byte[] encrypted = Encrypt(json);
            byte[] lenBytes = BitConverter.GetBytes(encrypted.Length);

            byte[] merged = lenBytes.Concat(encrypted).ToArray();
            List<int> bits = ToBits(merged);

            int bitIndex = 0;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height && bitIndex < bits.Count; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);

                    for (int x = 0; x < row.Length && bitIndex < bits.Count; x++)
                    {
                        ref Rgba32 p = ref row[x];

                        p.R = (byte)((p.R & 0xFE) | bits[bitIndex++]);
                        if (bitIndex >= bits.Count) break;

                        p.G = (byte)((p.G & 0xFE) | bits[bitIndex++]);
                        if (bitIndex >= bits.Count) break;

                        p.B = (byte)((p.B & 0xFE) | bits[bitIndex++]);
                    }
                }
            });

            await image.SaveAsync(physicalPath, new PngEncoder());

            return saveRelativePath.Replace("\\", "/");
        }

        // ==================================================
        // 2) GIẢI MÃ
        // ==================================================
        public async Task<string?> ExtractJsonFromImageAsync(string relativePath)
        {
            string fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (!File.Exists(fullPath))
                return null;

            using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(fullPath);

            List<int> bits = new();

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);

                    for (int x = 0; x < row.Length; x++)
                    {
                        Rgba32 p = row[x];

                        bits.Add(p.R & 1);
                        bits.Add(p.G & 1);
                        bits.Add(p.B & 1);
                    }
                }
            });

            // ---- READ LENGTH (FIRST 4 BYTE) ----
            byte[] lenBytes = BitsToBytes(bits.Take(32).ToList());
            int encLength = BitConverter.ToInt32(lenBytes);

            int totalBitsNeeded = (4 + encLength) * 8;
            byte[] mergedBytes = BitsToBytes(bits.Take(totalBitsNeeded).ToList());

            byte[] encrypted = mergedBytes.Skip(4).ToArray();

            try
            {
                return Decrypt(encrypted);
            }
            catch
            {
                return null;
            }
        }

        // ==================================================
        // AES
        // ==================================================
        private byte[] Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_opt.EncryptionKey);
            aes.IV = Encoding.UTF8.GetBytes(_opt.Iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);

            sw.Write(plain);
            sw.Close();

            return ms.ToArray();
        }

        private string Decrypt(byte[] encrypted)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_opt.EncryptionKey);
            aes.IV = Encoding.UTF8.GetBytes(_opt.Iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream(encrypted);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        // ==================================================
        // BIT CONVERSION
        // ==================================================
        private List<int> ToBits(byte[] data)
        {
            List<int> bits = new();
            foreach (var b in data)
                for (int i = 7; i >= 0; i--)
                    bits.Add((b >> i) & 1);
            return bits;
        }

        private byte[] BitsToBytes(List<int> bits)
        {
            int count = bits.Count / 8;

            byte[] result = new byte[count];

            for (int i = 0; i < count; i++)
            {
                byte b = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    b = (byte)((b << 1) | bits[i * 8 + bit]);
                }
                result[i] = b;
            }

            return result;
        }
    }
}
