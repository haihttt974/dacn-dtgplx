using System.Text.Json.Serialization;

namespace dacn_dtgplx.ViewModels
{
    public class Mat10VM
    {
        [JsonPropertyName("mat_trai(10)")]
        public string? MatTrai10 { get; set; }

        [JsonPropertyName("mat_phai(10)")]
        public string? MatPhai10 { get; set; }
    }
}
