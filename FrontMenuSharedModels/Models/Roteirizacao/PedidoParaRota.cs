using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Roteirizacao;

public class PedidoParaRota
{
    [JsonPropertyName("pedidoId")] public int PedidoId { get; set; }
    [JsonPropertyName("cliente")] public string Cliente { get; set; } = "";
    [JsonPropertyName("endereco")] public string Endereco { get; set; } = "";
    [JsonPropertyName("telefone")] public string? Telefone { get; set; }
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
    [JsonPropertyName("taxaEntrega")] public decimal TaxaEntrega { get; set; }
    [JsonPropertyName("lat")] public double? Lat { get; set; }
    [JsonPropertyName("lng")] public double? Lng { get; set; }
    [JsonPropertyName("erroGeocode")] public bool ErroGeocode { get; set; }
    [JsonPropertyName("displayId")] public string DisplayId { get; set; } = "";
    [JsonPropertyName("formaPagamento")] public string FormaPagamento { get; set; } = "";
    [JsonPropertyName("pagamentoOnline")] public bool PagamentoOnline { get; set; }
    [JsonPropertyName("rua")] public string Rua { get; set; } = "";
    [JsonPropertyName("numero")] public string Numero { get; set; } = "";
    [JsonPropertyName("bairro")] public string Bairro { get; set; } = "";
    [JsonPropertyName("cidade")] public string Cidade { get; set; } = "";
    [JsonPropertyName("estado")] public string Estado { get; set; } = "";
    [JsonPropertyName("complemento")] public string Complemento { get; set; } = "";
    [JsonPropertyName("referencia")] public string Referencia { get; set; } = "";
    
    [JsonIgnore] public string Zona { get; set; } = "Disponiveis";
}
