using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models;

public class Merchant
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("razaoSocial")] public string RazaoSocial { get; set; } = string.Empty;
    [JsonPropertyName("ImagemLogo")] public string ImagemLogo { get; set; } = string.Empty;
    [JsonPropertyName("NomeFantasia")] public string NomeFantasia { get; set; } = string.Empty;
    [JsonPropertyName("marcaDepartamento")] public string MarcaDepartamento { get; set; } = string.Empty;
    [JsonPropertyName("legendaDoVoluma")] public string LegendaDoVoluma { get; set; } = string.Empty;
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } 
    [JsonPropertyName("CertificadoBase64")] public string? CertificadoBase64 { get; set; }
    [JsonPropertyName("SenhaCertificado")] public string? SenhaCertificado { get; set; }
}
