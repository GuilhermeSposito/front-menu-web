using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class PagamentoDoPedido
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ValorDosItens")] public float ValorDosItens { get; set; }
    [JsonPropertyName("TaxaEntregaValor")] public float TaxaEntregaValor { get; set; }
    [JsonPropertyName("DescontoValor")] public float DescontoValor { get; set; }
    [JsonPropertyName("AcrescimoValor")] public float AcrescimoValor { get; set; }
    [JsonPropertyName("ServicoValor")] public float ServicoValor { get; set; }
    [JsonPropertyName("Troco")] public float Troco { get; set; }
    [JsonPropertyName("ValorTotal")] public float ValorTotal { get; set; }
    [JsonPropertyName("IncentivosExternosValor")] public float IncentivosExternosValor { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime CriadoEm { get; set; }
}
