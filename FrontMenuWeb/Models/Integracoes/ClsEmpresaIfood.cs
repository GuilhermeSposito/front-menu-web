using FrontMenuWeb.Models.Merchant;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Integracoes;

public class ClsEmpresaIfood
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("NomeEmpresa")] public string NomeEmpresa { get; set; } = string.Empty;
    [JsonPropertyName("MerchantIdIfood")] public string MerchantIdIfood { get; set; } = string.Empty;
    [JsonPropertyName("online")] public bool Online { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("merchant")] public ClsMerchant? MerchantSophos { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("CodigoDeAutorizacaoDoIfood")] public string? CodigoDeAutorizacaoDoIfood { get; set; }

}
