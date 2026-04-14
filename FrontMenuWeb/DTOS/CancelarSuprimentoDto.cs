using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class CancelarSuprimentoDto
{
    [JsonPropertyName("motivoCancelamento")] public string MotivoCancelamento { get; set; } = string.Empty;
}
