using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class FracionarItemMesaRequestDto
{
    [JsonPropertyName("itemId")] public int ItemId { get; set; }
    [JsonPropertyName("quantidadeASeparar")] public decimal QuantidadeASeparar { get; set; }
}
