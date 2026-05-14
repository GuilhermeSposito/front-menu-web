using FrontMenuWeb.Models.Merchant;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class DelmatchEmpresaDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("IdExterno")] public int IdExterno { get; set; }
    [JsonPropertyName("Email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("AccessToken")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("DataExpiracaoToken")] public DateTime DataExpiracaoToken { get; set; }
    [JsonPropertyName("Ativo")] public bool Ativo { get; set; }
    [JsonPropertyName("merchant")] public ClsMerchant? MerchantSophos { get; set; }
}

public class DelmatchEmpresasPollingResponseDto
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("Empresas")] public List<DelmatchEmpresaDto> Empresas { get; set; } = new();
}

public class DelmatchEmpresaByIdResponseDto
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("Empresa")] public DelmatchEmpresaDto? Empresa { get; set; }
}
