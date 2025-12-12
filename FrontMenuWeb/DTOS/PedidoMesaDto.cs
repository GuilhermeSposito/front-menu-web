using FrontMenuWeb.Models.Pedidos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class PedidoMesaDto
{
    [JsonPropertyName("IdentificacaoMesaOuComanda")] public int IdentificacaoMesaOuComanda { get; set; }
    [JsonPropertyName("NomeCliente")]public string? NomeCliente { get; set; }
    [JsonPropertyName("Itens")] public List<ItensPedido> Itens { get; set; } = new List<ItensPedido>();
}
