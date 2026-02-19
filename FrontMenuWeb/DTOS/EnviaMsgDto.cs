using FrontMenuWeb.Models.Pedidos;

namespace FrontMenuWeb.DTOS;

public class EnviaMsgDto
{
    public EtapasPedido EtapaDoPedido { get; set; }
    public string NumeroDoPedido { get; set; } = string.Empty;
    public ClsPedido Pedido { get; set; } = new ClsPedido();
}
