using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;

namespace FrontMenuWeb.Models.Cardapio;

public class PedidoState
{
    public ClsMerchant? Merchant { get; set; }
    public List<ClsFormaDeRecebimento>? FormasDeRecebimento { get; set; }
    public ClsPedido? NovoPedido { get; set; }
}