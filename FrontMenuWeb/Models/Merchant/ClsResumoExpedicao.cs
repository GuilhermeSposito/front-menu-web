using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsResumoExpedicao
{
    [JsonPropertyName("geradoEm")] public DateTime GeradoEm { get; set; } = DateTime.Now;
    [JsonPropertyName("quantidadePedidos")] public int QuantidadePedidos { get; set; }
    [JsonPropertyName("totalPedidos")] public float TotalPedidos { get; set; }
    [JsonPropertyName("totalEntregas")] public float TotalEntregas { get; set; }
    [JsonPropertyName("motoboys")] public List<ClsResumoMotoboy> Motoboys { get; set; } = new();
    [JsonPropertyName("pagamentos")] public List<ClsResumoPagamento> Pagamentos { get; set; } = new();
    [JsonPropertyName("trocos")] public ClsResumoTrocos Trocos { get; set; } = new();
    [JsonPropertyName("pedidos")] public List<ClsResumoPedidoFechado> Pedidos { get; set; } = new();
}

public class ClsResumoMotoboy
{
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("Telefone")] public string Telefone { get; set; } = string.Empty;
    [JsonPropertyName("quantidadePedidos")] public int QuantidadePedidos { get; set; }
    [JsonPropertyName("totalPedidos")] public float TotalPedidos { get; set; }
    [JsonPropertyName("totalEntregas")] public float TotalEntregas { get; set; }
}

public class ClsResumoPagamento
{
    [JsonPropertyName("formaDePagamento")] public string FormaDePagamento { get; set; } = string.Empty;
    [JsonPropertyName("quantidade")] public int Quantidade { get; set; }
    [JsonPropertyName("total")] public float Total { get; set; }
    [JsonPropertyName("totalTrocos")] public float TotalTrocos { get; set; }
}

public class ClsResumoTrocos
{
    [JsonPropertyName("houveTroco")] public bool HouveTroco { get; set; }
    [JsonPropertyName("totalTrocos")] public float TotalTrocos { get; set; }
    [JsonPropertyName("quantidadePedidosComTroco")] public int QuantidadePedidosComTroco { get; set; }
}

public class ClsResumoPedidoFechado
{
    [JsonPropertyName("displayId")] public string DisplayId { get; set; } = string.Empty;
    [JsonPropertyName("motoboy")] public string Motoboy { get; set; } = string.Empty;
    [JsonPropertyName("formasDeRecebimento")] public string FormasDeRecebimento { get; set; } = string.Empty;
    [JsonPropertyName("taxaMotoboy")] public float TaxaMotoboy { get; set; }
    [JsonPropertyName("valorTotal")] public float ValorTotal { get; set; }
}
