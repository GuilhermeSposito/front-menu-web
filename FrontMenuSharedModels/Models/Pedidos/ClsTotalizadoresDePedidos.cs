using System.Text.Json.Serialization;

public class ClsTotalizadoresDePedidos
{
    [JsonPropertyName("TotalPedidos")] public int TotalPedidos { get; set; }
    [JsonPropertyName("ValorTotal")] public float ValorTotal { get; set; }
    [JsonPropertyName("TotalTaxaEntrega")] public float TotalTaxaEntrega { get; set; }
    [JsonPropertyName("TotalDesconto")] public float TotalDesconto { get; set; }
    [JsonPropertyName("TotalServico")] public float TotalServico { get; set; }
    [JsonPropertyName("TotalAcrescimo")] public float TotalAcrescimo { get; set; }
    [JsonPropertyName("TotalItens")] public float TotalItens { get; set; }
    [JsonPropertyName("TotalIncentivos")] public float TotalIncentivos { get; set; }
    [JsonPropertyName("TicketMedio")] public float TicketMedio { get; set; }
    [JsonPropertyName("TotalPedidosBalcao")] public int TotalPedidosBalcao { get; set; }
    [JsonPropertyName("TotalPedidosMesa")] public int TotalPedidosMesa { get; set; }
    [JsonPropertyName("TotalPedidosDelivery")] public int TotalPedidosDelivery { get; set; }
    [JsonPropertyName("MediaTaxaEntregaPorPedido")] public float MediaTaxaEntregaPorPedido { get; set; }
}
