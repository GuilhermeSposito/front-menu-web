using FrontMenuWeb.Models.Pedidos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class ClsCancelationReasons
{
    [JsonPropertyName("cancelCodeId")] public string? CancelCodeId { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }

}

public class ClsCancalationComfirmation
{
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("cancellationCode")] public string? CancellationCode { get; set; }
}

public class CancelationIfoodObjectDto
{
    [JsonPropertyName("Pedido")] public ClsPedido? Pedido { get; set; }
    [JsonPropertyName("CancelationObject")] public ClsCancalationComfirmation CancalationComfirmation { get; set; } = new ClsCancalationComfirmation();
}