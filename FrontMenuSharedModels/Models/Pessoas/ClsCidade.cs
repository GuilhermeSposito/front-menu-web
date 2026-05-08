using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Pessoas;

public class ClsCidade
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("numCidade")] public int NumCidade { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
}
