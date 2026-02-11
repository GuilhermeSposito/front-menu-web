using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Fiscal;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Vendas;

public class Caixa
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ValorInicial")] public decimal ValorInicial { get; set; }
    [JsonPropertyName("SangriasValor")] public decimal SangriasValor { get; set; }
    [JsonPropertyName("suprimentos_valor")] public decimal SuprimentoValor { get; set; }
    [JsonPropertyName("ValorEmCaixa")] public decimal ValorEmCaixa { get; set; }
    [JsonPropertyName("ValorTotalDasVendas")] public decimal ValorTotalDasVendas { get; set; }
    [JsonPropertyName("FaltouOuSobrou")] public decimal FaltouOuSobrou { get; set; }
    [JsonPropertyName("ValorTaxasDeEntrega")] public decimal ValorTaxasDeEntrega { get; set; }
    [JsonPropertyName("ValorDescontos")] public decimal ValorDescontos { get; set; }
    [JsonPropertyName("ValorAcrescimos")] public decimal ValorAcrescimos { get; set; }
    [JsonPropertyName("ValorIncentivos")] public decimal ValorIncentivos { get; set; }
    [JsonPropertyName("ValorTrocos")] public decimal ValorTrocos { get; set; }
    [JsonPropertyName("ValorCaixaEmDinFinal")] public decimal ValorCaixaEmDinFinal { get; set; }
    [JsonPropertyName("TiketMedio")] public decimal TiketMedio { get; set; }
    [JsonPropertyName("Aberto")] public bool Aberto { get; set; }
    [JsonPropertyName("DataAbertura")] public DateTime? DataAbertura { get; set; }
    [JsonPropertyName("DataFechamento")] public DateTime? DataFechamento { get; set; }
    [JsonPropertyName("FuncionarioAbertura")] public ClsFuncionario? FuncionarioAbertura { get; set; }
    [JsonPropertyName("FuncionarioFechamento")] public ClsFuncionario? FuncionarioFechamento { get; set; }
    [JsonPropertyName("MovimentosDeCaixa")] public List<PagamentoDoPedido> MovimentosDeCaixa { get; set; } = new List<PagamentoDoPedido>();
    [JsonPropertyName("Pedidos")] public List<ClsPedido> Pedidos { get; set; } = new List<ClsPedido>();
    [JsonPropertyName("NFS")] public List<NfeReturnDto> NfsEmitidas { get; set; } = new List<NfeReturnDto>();
    [JsonPropertyName("ItensPedido")] public List<ItensPedido> ItensPedido { get; set; } = new List<ItensPedido>();
    
}
