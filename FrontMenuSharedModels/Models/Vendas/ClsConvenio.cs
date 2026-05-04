using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Pedidos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Vendas;

public class ClsConvenio
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Pessoa")] public ClsPessoas? Pessoa { get; set; }
    [JsonPropertyName("SaldoDevedor")] public float SaldoDevedor { get; set; }
    [JsonPropertyName("LimiteCreditoSnapshot")] public float LimiteCreditoSnapshot { get; set; }
    [JsonPropertyName("Status")] public string Status { get; set; } = "ATIVO";
    [JsonPropertyName("CriadoEm")] public DateTime CriadoEm { get; set; }
    [JsonPropertyName("Pedidos")] public List<ClsConvenioPedido> Pedidos { get; set; } = new();
    [JsonPropertyName("Pagamentos")] public List<ClsConvenioPagamento> Pagamentos { get; set; } = new();
}

public class ClsConvenioPedido
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Pedido")] public ClsPedido? Pedido { get; set; }
    [JsonPropertyName("ValorPedido")] public float ValorPedido { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime CriadoEm { get; set; }
}

public class ClsConvenioPagamento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("FormaDeRecebimento")] public ClsFormaDeRecebimento? FormaDeRecebimento { get; set; }
    [JsonPropertyName("ValorPago")] public float ValorPago { get; set; }
    [JsonPropertyName("Troco")] public float Troco { get; set; }
    [JsonPropertyName("PagoEm")] public DateTime PagoEm { get; set; }
    [JsonPropertyName("Obs")] public string? Obs { get; set; }
}

public class CriarPagamentoConvenioDto
{
    [JsonPropertyName("PessoaId")] public int PessoaId { get; set; }
    [JsonPropertyName("FormaDeRecebimentoId")] public int FormaDeRecebimentoId { get; set; }
    [JsonPropertyName("ValorPago")] public float ValorPago { get; set; }
    [JsonPropertyName("Troco")] public float Troco { get; set; }
    [JsonPropertyName("Obs")] public string? Obs { get; set; }
}

public class ConveniosPagedResponse
{
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("data")] public ConveniosPagedData Data { get; set; } = new();
}

public class ConveniosPagedData
{
    [JsonPropertyName("data")] public List<ClsConvenio> Data { get; set; } = new();
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("limit")] public int Limit { get; set; }
}
