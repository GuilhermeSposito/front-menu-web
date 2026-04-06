using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsMotoboy
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("celular")] public string? Celular { get; set; }
    [JsonPropertyName("cpf")] public string? Cpf { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } = true;
    [JsonPropertyName("criadoEm")] public DateTime? CriadoEm { get; set; }
}
