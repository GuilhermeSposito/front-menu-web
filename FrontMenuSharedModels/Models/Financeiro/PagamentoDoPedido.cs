using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class PagamentoDoPedido
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ValorDosItens")] public float ValorDosItens { get; set; }

    [JsonPropertyName("Troco")] public float Troco { get; set; }
    [JsonPropertyName("ValorTotal")] public float ValorTotal { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime CriadoEm { get; set; } = DateTime.Now;
    [JsonPropertyName("FormaDeRecebimento")] public ClsFormaDeRecebimento? FormaDePagamento { get; set; }
    [JsonPropertyName("FormaDeRecebimentoId")] public int formaDeRecebimentoId { get; set; }
    [JsonPropertyName("CupomFiscalChave")] public string? CupomFiscalChave { get; set; }
    [JsonPropertyName("TaxaEntrega")] public float TaxaEntrega { get; set; }
    [JsonPropertyName("Desconto")] public float Desconto { get; set; }
    [JsonPropertyName("Acrescimo")] public float Acrescimo { get; set; }
    [JsonPropertyName("Servico")] public float Servico { get; set; }
    [JsonPropertyName("IncentivosExternos")] public float IncentivosExternos { get; set; }
    [JsonPropertyName("Couvert")] public float Couvert { get; set; }
}
