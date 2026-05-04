using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class EntradaEstoqueDto
{
    [JsonPropertyName("Quantidade")]
    public float Quantidade { get; set; }

    [JsonPropertyName("PrecoCusto")]
    public float? PrecoCusto { get; set; }
}
