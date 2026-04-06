using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsPedidoMotoboy
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("motoboyId")] public int MotoboyId { get; set; }

    // Flat FK used when POSTing (the server resolves pedido_id from this)
    [JsonPropertyName("pedidoCaixaId")] public int PedidoCaixaId { get; set; }

    // Nested object returned by GET (TypeORM serializes relations as nested objects)
    [JsonPropertyName("pedidoCaixa")] public ClsPedidoCaixaRef? PedidoCaixaObj { get; set; }

    [JsonPropertyName("ValorPedido")] public float ValorPedido { get; set; }
    [JsonPropertyName("ValorEntrega")] public float ValorEntrega { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime? CriadoEm { get; set; }
    [JsonPropertyName("motoboy")] public ClsMotoboy? Motoboy { get; set; }

    // Resolves the PedidoCaixa id regardless of whether it came from a nested object or a flat field
    [JsonIgnore]
    public int PedidoCaixaIdFinal => PedidoCaixaObj?.Id ?? PedidoCaixaId;
}

public class ClsPedidoCaixaRef
{
    [JsonPropertyName("id")] public int Id { get; set; }
}

public class ClsDistanciaEntrega
{
    [JsonPropertyName("valorMotoboy")] public float ValorMotoboy { get; set; }
    [JsonPropertyName("valorTotal")] public float ValorTotal { get; set; }
    [JsonPropertyName("distanciaKm")] public float DistanciaKm { get; set; }
    [JsonPropertyName("distanciaTexto")] public string DistanciaTexto { get; set; } = string.Empty;
    [JsonPropertyName("duracaoMinutos")] public int DuracaoMinutos { get; set; }
    [JsonPropertyName("duracaoTexto")] public string DuracaoTexto { get; set; } = string.Empty;
}
