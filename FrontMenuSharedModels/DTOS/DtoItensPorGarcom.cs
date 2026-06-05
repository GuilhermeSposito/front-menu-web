using FrontMenuWeb.Models.Pedidos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class DtoItensPorGarcom
{
    [JsonPropertyName("data")] public List<ItensPedido> Itens { get; set; } = new();
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("lastPage")] public int LastPage { get; set; }
    [JsonPropertyName("valorTotalVendas")] public double ValorTotalVendas { get; set; }
    [JsonPropertyName("taxaServicoPercent")] public double TaxaServicoPercent { get; set; }
    [JsonPropertyName("valorTaxaServico")] public double ValorTaxaServico { get; set; }
    [JsonPropertyName("valorTotalComTaxa")] public double ValorTotalComTaxa { get; set; }
}
