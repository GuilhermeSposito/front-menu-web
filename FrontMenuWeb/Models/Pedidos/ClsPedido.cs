using FrontMenuWeb.Models.Financeiro;
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
    [Display(Name = "ABERTO")] ABERTO,
    FECHANDO,
    FECHADO,
    CANCELADO,
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
    [JsonPropertyName("CriadoPor")] public string CriadoPor { get; set; } = "SOPHOS";
    [JsonPropertyName("TipoPedido")] public string TipoDePedido { get; set; } = "BALCÃO";
    [JsonPropertyName("Etapa")] public string EtapaPedido { get; set; } = "NOVO";
    [JsonPropertyName("Status")] public string StatusPedido { get; set; } = "FECHADO";
    [JsonPropertyName("Itens")] public List<ItensPedido> Itens { get; set; } = new List<ItensPedido>();
    private ClsPessoas? _cliente;

    [JsonPropertyName("cliente")]
    public ClsPessoas? Cliente
    {
        get => _cliente;
        set
        {
            _cliente = value;
            ClienteId = value?.Id ?? 0; // Atualiza automaticamente
        }
    }
    [JsonPropertyName("ClienteId")] public int ClienteId { get; set; }
    private EnderecoPessoa? _endereco;
    [JsonPropertyName("endereco")]
    public EnderecoPessoa? Endereco
    {
        get => _endereco;
        set
        {
            _endereco = value;
            EnderecoId = value?.Id ?? 0; // Atualiza automaticamente
        }
    }

    [JsonPropertyName("EnderecoId")]
    public int EnderecoId { get; set; }

    [JsonPropertyName("Pagamentos")] public List<PagamentoDoPedido> Pagamentos { get; set; } = new List<PagamentoDoPedido>();
    [JsonPropertyName("ValorDosItens")] public float ValorDosItens { get; set; }
    [JsonPropertyName("TaxaEntregaValor")] public float TaxaEntregaValor { get; set; }
    [JsonPropertyName("DescontoValor")] public float DescontoValor { get; set; }
    [JsonPropertyName("AcrescimoValor")] public float AcrescimoValor { get; set; }
    [JsonPropertyName("ServicoValor")] public float ServicoValor { get; set; }
    [JsonPropertyName("IncentivosExternosValor")] public float IncentivosExternosValor { get; set; }
    [JsonPropertyName("ValorTotal")] public float ValorTotal { get; set; }

}

public class ItensPedido
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Produto")]
    private ClsProduto? _produto;
    public ClsProduto? Produto
    {
        get => _produto;
        set
        {
            _produto = value;
            ProdutoId = value?.Id; // Atualiza o ID quando Produto é atribuído
        }
    }
    [JsonPropertyName("ProdutoId")] public string? ProdutoId { get; set; }
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; } = 1;
    [JsonPropertyName("PrecoUnitario")] public float PrecoUnitario { get; set; } = 0;
    [JsonPropertyName("PrecoTotal")] public float PrecoTotal { get; set; } = 0;
    [JsonPropertyName("Preco")] public Preco Preco { get; set; } = new Preco();
    [JsonPropertyName("PrecoId")] public int PrecoId { get; set; }
    [JsonPropertyName("Observacoes")] public string Observacoes { get; set; } = string.Empty;
    [JsonPropertyName("Complementos")] public List<ComplementoNoItem> Complementos { get; set; } = new List<ComplementoNoItem>();
}

public class ComplementoNoItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    private ClsComplemento? _complemento;

    [JsonPropertyName("Complemento")]
    public ClsComplemento? Complemento
    {
        get => _complemento;
        set
        {
            _complemento = value;
            ComplementoId = value?.Id ?? 0; // Atualiza o ID quando o complemento é atribuído
        }
    }

    [JsonPropertyName("ComplementoId")]
    public int ComplementoId { get; set; }

    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; } = 0;
    [JsonPropertyName("PrecoUnitario")] public float PrecoUnitario { get; set; }
    [JsonPropertyName("PrecoTotal")] public float PrecoTotal { get; set; }
}