using System.Text.Json.Serialization;

namespace dacn_dtgplx.ViewModels
{
    public class HealthInfoVM
    {
        [JsonPropertyName("anh_giay_kham")]
        public List<string> AnhGiayKham { get; set; } = new();

        [JsonPropertyName("thoi_han")]
        public string? ThoiHan { get; set; }

        [JsonPropertyName("mat")]
        public Mat10VM? Mat { get; set; }

        [JsonPropertyName("huyet_ap(120)")]
        public string? HuyetAp120 { get; set; }

        [JsonPropertyName("chieu_cao (cm)")]
        public string? ChieuCaoCm { get; set; }

        [JsonPropertyName("can_nang (kg)")]
        public string? CanNangKg { get; set; }
    }
}
