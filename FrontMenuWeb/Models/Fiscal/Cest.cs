using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Fiscal;

public class Cest
{
    [JsonPropertyName("id")] public int id { get; set; }
    [JsonPropertyName("CodigoNcm")] public string? CodigoNcm { get; set; } = string.Empty;
    [JsonPropertyName("Cest")] public string? CestReferente { get; set; } = string.Empty;
    [JsonPropertyName("Segmento")] public string? Segmento { get; set; } = string.Empty;
    [JsonPropertyName("Descricao")] public string? Descricao { get; set; } = string.Empty;
}
