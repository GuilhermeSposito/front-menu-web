using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class AutoCompleteDeEndereco
{
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("termos")] public List<string> Termos { get; set; } = new List<string>();
}
