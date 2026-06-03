using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class CreateMerchantDto
{
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("senha")] public string Senha { get; set; } = string.Empty;
    [JsonPropertyName("ImagemLogo")] public string? ImagemLogo { get; set; }
    [JsonPropertyName("razaoSocial")] public string RazaoSocial { get; set; } = string.Empty;
    [JsonPropertyName("NomeFantasia")] public string NomeFantasia { get; set; } = string.Empty;
    [JsonPropertyName("EmitindoNfeProd")] public bool EmitindoNfeProd { get; set; } = false;
}
