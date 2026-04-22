using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Roteirizacao;

public class OtimizarRotaDto
{
    [JsonPropertyName("origemLat")] public double OrigemLat { get; set; }
    [JsonPropertyName("origemLng")] public double OrigemLng { get; set; }
    [JsonPropertyName("pontos")] public List<PontoDto> Pontos { get; set; } = new();
    [JsonPropertyName("voltarAoOrigem")] public bool VoltarAoOrigem { get; set; } = true;
}

public class PontoDto
{
    [JsonPropertyName("pedidoId")] public int PedidoId { get; set; }
    [JsonPropertyName("lat")] public double Lat { get; set; }
    [JsonPropertyName("lng")] public double Lng { get; set; }
}
