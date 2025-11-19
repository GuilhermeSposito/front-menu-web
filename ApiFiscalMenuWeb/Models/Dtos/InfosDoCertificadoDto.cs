using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class InfosDoCertificadoDto
{
    [JsonPropertyName("CertificadoEmBase64")] public string CertificadoEmBase64 { get; set; } = string.Empty;
    [JsonPropertyName("SenhaCertificado")] public string SenhaCertificado { get; set; } = string.Empty;
}
