using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Integracoes;

public class ClsEmpresaIfood
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("NomeEmpresa")] public string NomeEmpresa { get; set; } = string.Empty;
    [JsonPropertyName("MerchantIdIfood")] public string MerchantIdIfood { get; set; } = string.Empty;
    [JsonPropertyName("AccessTokenIfood")] public string AccessTokenIfood { get; set; } = string.Empty;
    [JsonPropertyName("RefreshTokenIfood")] public string RefreshTokenIfood { get; set; } = string.Empty;
    [JsonPropertyName("VenceTokenIfood")] public DateTime VenceTokenIfood { get; set; }
    [JsonPropertyName("online")] public bool Online { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }

}
