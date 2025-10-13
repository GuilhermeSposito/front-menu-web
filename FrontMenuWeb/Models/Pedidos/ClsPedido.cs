using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Pedidos;

public enum TiposDePedido
{
    [Display(Name = "BALCÃO")] BALCAO,
    MESA,
    DELIVERY,
    RETIRADA,
}

public enum EtapasPedido
{
    NOVO,
    PREPARANDO,
    DESPACHADO,
    PRONTO,
    FINALIZADO
}

public enum StatusPedidos
{
    ABERTO,
    FECHANDO,
    FECHADO,
    CANCELADO,
    ENTREGUE,
    DESPACHADO,
    FINALIZADO
}

public class ClsDeSuporteParaMostrarPedidos
{
    public ClsPedido Pedido { get; set; } = new ClsPedido();
    public bool Selecionado { get; set; } = false;
    public bool Expandido { get; set; } = false;
    public string Selector { get; set; } = string.Empty;
    public bool UltimaEtapa { get; set; } = false;
}

public class ClsPedido
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime CriadoEm { get; set; }
    [JsonPropertyName("ModificadoEm")] public DateTime ModificadoEm { get; set; }
    [JsonPropertyName("CriadoPor")] public string CriadoPor { get; set; } = string.Empty;
    [JsonPropertyName("tipoDePedido")] public string TipoDePedido { get; set; } = "BALCÃO";
    [JsonPropertyName("EtapaPedido")] public string EtapaPedido { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string StatusPedido { get; set; } = string.Empty;
    [JsonPropertyName("Itens")] public List<ItensPedido> Itens { get; set; } = new List<ItensPedido>();
    [JsonPropertyName("cliente")] public ClsPessoas? Cliente { get; set; } = new ClsPessoas();

}

public class ItensPedido
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Produto")] public ClsProduto? Produto { get; set; }
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; } = 1;
    [JsonPropertyName("PrecoUnitario")] public float PrecoUnitario { get; set; } = 0;
    [JsonPropertyName("PrecoTotal")] public float PrecoTotal { get; set; } = 0;
    [JsonPropertyName("Preco")] public Preco Preco { get; set; } = new Preco();
    [JsonPropertyName("Observacoes")] public string Observacoes { get; set; } = string.Empty;
    [JsonPropertyName("Complementos")] public List<ComplementoNoItem> Complementos { get; set; } = new List<ComplementoNoItem>();
}

public class ComplementoNoItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Produto")] public ClsComplemento? Complemento { get; set; }
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; } = 0;
    [JsonPropertyName("PrecoUnitario")] public float PrecoUnitario { get; set; }
    [JsonPropertyName("PrecoTotal")] public float PrecoTotal { get; set; }
}