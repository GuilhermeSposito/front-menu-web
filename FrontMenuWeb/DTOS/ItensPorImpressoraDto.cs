using FrontMenuWeb.Models.Pedidos;

namespace FrontMenuWeb.DTOS;

public class ItensPorImpressoraDto
{
    public string? Impressora { get; set; }
    public List<ItensPedido> Itens { get; set; } = new();
}
