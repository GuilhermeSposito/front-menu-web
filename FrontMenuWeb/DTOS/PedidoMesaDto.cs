using FrontMenuWeb.Models.Pedidos;

namespace FrontMenuWeb.DTOS;

public class PedidoMesaDto
{
    public int IdentificacaoMesaOuComanda { get; set; }
    public string? NomeCliente { get; set; }
    public List<ItensPedido> Itens { get; set; } = new List<ItensPedido>();
}
