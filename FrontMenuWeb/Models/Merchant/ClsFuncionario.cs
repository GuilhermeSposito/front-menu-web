using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsFuncionario
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
}
