using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class AdicionarCouvertRequestDto
{
    [JsonPropertyName("QtdPessoas")] public int QtdPessoas { get; set; }
    [JsonPropertyName("NomeCliente")] public string? NomeCliente { get; set; }
    [JsonPropertyName("ValorUnitario")] public decimal? ValorUnitario { get; set; }
}
