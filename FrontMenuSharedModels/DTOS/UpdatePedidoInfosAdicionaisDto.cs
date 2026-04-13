using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class UpdatePedidoInfosAdicionaisDto
{
    [JsonPropertyName("InfoAdicionalOuStatus")] public string InfoAdicionalOuStatus { get; set; } = string.Empty;
}
