using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Pedidos;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class UpdatePedidosDto
{
    [JsonPropertyName("Pedido")] public ClsPedido? Pedido { get; set; }
    [JsonPropertyName("EmpresaIfood")] public ClsEmpresaIfood MerchantIfood { get; set; } = new();
    [JsonPropertyName("PedidoIdIntegracao")] public string PedidoIdIntegracao { get; set; } = string.Empty;
    [JsonPropertyName("DestinoPedido")] public DestinoPedido DestinoPedido { get; set; }
    [JsonPropertyName("MerchantId")] public string? MerchantId { get; set; }
    [JsonIgnore] public string? TokenNestApi { get; set; }
}


public enum DestinoPedido
{
    Sophos = 0,
    Ifood = 1
}

