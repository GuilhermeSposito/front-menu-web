using FrontMenuWeb.Models.Merchant;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Integracoes;

public class ClsEmpresaDelmatch
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("IdExterno")] public int IdExterno { get; set; }
    [JsonPropertyName("Email")] public string Email { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("Senha")] public string? Senha { get; set; }
    [JsonPropertyName("Ativo")] public bool Ativo { get; set; } = true;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("merchant")] public ClsMerchant? MerchantSophos { get; set; }
}
