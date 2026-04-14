using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class CreateSuprimentoDto
{
    [JsonPropertyName("caixaId")] public int CaixaId { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
}
