namespace dacn_dtgplx.Helpers
{
    public static class ImageHelper
    {
        public static string? Normalize(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            fileName = fileName.Trim();

            if (fileName.StartsWith("wwwwroot"))
                fileName = fileName.Replace("wwwwroot", "").TrimStart('/');

            if (fileName.StartsWith("wwwroot"))
                fileName = fileName.Replace("wwwroot", "").TrimStart('/');

            if (fileName.StartsWith("~/"))
                return fileName.Replace("~/", "/");

            if (fileName.StartsWith("images"))
                return "/" + fileName;

            if (fileName.StartsWith("/images"))
                return fileName;

            return "/images/bien_bao/" + fileName;
        }
    }
}
