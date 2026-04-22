using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Roteirizacao;

public class RotaOtimizada
{
    [JsonPropertyName("ordemOtimizada")] public List<int> OrdemOtimizada { get; set; } = new();
    [JsonPropertyName("distanciaKm")] public double DistanciaKm { get; set; }
    [JsonPropertyName("tempoMin")] public double TempoMin { get; set; }
}
