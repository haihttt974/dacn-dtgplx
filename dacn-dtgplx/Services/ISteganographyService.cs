using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace dacn_dtgplx.Services
{
    public interface ISteganographyService
    {
        /// <summary>
        /// Mã hoá JSON rồi giấu vào ảnh (LSB) và lưu ảnh ra file PNG.
        /// Trả về đường dẫn tương đối (relative) để lưu DB.
        /// </summary>
        Task<string> HideJsonIntoImageAsync(
            IFormFile uploadImage,
            string saveRelativePath,
            string json);

        /// <summary>
        /// Đọc ảnh PNG, giải mã steganography + AES và trả về JSON gốc.
        /// </summary>
        Task<string?> ExtractJsonFromImageAsync(string relativePath);
    }
}
