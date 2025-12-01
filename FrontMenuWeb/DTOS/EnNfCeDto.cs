using FrontMenuWeb.Models.Pedidos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Dtos;

public class EnNfCeDto
{
    [JsonPropertyName("cpf")] public string? CPF { get; set; }
    [JsonPropertyName("cnpj")] public string? CNPJ { get; set; }
    [JsonPropertyName("nomeCliente")] public string? NomeCliente { get; set; }
    [Required(ErrorMessage = "O pedido é obrigatório")][JsonPropertyName("pedido")] public ClsPedido Pedido { get; set; } = new();
}
