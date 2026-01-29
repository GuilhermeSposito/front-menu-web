using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Raios;

public class RaioDeEntrega
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Km")] public int Km { get; set; }
    [JsonPropertyName("ValorTaxa")] public decimal ValorTaxa { get; set; }
    [JsonPropertyName("valor_taxa")] public decimal valor_taxa { get; set; }
    [JsonPropertyName("ValorTaxaMotoboy")] public decimal ValorTaxaMotoboy { get; set; }
    [JsonPropertyName("TempoMinutos")] public int TempoMinutos { get; set; }
    [JsonPropertyName("Ativo")] public bool Ativo { get; set; } = true;
}
