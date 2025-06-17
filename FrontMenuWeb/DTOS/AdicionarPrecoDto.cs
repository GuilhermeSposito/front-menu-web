using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class AdicionarPrecoDto
{
    [JsonPropertyName("DescricaoTamanho")] public string? DescricaoDoTamanho { get; set; }
    [JsonPropertyName("CustosInsumo")] public float? CustosDoInsumo { get; set; }
    [JsonPropertyName("CustoReal")] public float? CustoReal { get; set; }
    [JsonPropertyName("PrecoSujetido")] public float? PrecoSujetido { get; set; }
    [JsonPropertyName("PorcentagemDeLucro")] public float? PorcentagemDeLucro { get; set; }
    [JsonPropertyName("Valor")] public double Valor { get; set; }
}
