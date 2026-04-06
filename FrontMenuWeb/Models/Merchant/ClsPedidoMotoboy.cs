using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsPedidoMotoboy
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("motoboyId")] public int MotoboyId { get; set; }
    [JsonPropertyName("pedidoId")] public int PedidoId { get; set; }
    [JsonPropertyName("pedidoCaixaId")] public int? PedidoCaixaId { get; set; }
    [JsonPropertyName("valorPedido")] public float ValorPedido { get; set; }
    [JsonPropertyName("valorEntrega")] public float ValorEntrega { get; set; }
    [JsonPropertyName("criadoEm")] public DateTime? CriadoEm { get; set; }
}
