using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Produtos;

public class ClsProduto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("codigo_interno")] public string? CodigoInterno { get; set; }
    [JsonPropertyName("categoria")] public Categoria? Categoria { get; set; }
    [JsonPropertyName("categoria_id")] public int? CategoriaId { get; set; }
    [JsonPropertyName("grupo_id")] public int GrupoId { get; set; }
    [JsonPropertyName("descricao")] public string? Descricao { get; set; }
    [JsonPropertyName("ncm")] public string? NCM { get; set; }
    [JsonPropertyName("cest")] public string? CEST { get; set; }

    [JsonPropertyName("cod_barra")] public string? CodBarras { get; set; }
    [JsonPropertyName("imagem_produto")] public string? ImgProduto { get; set; }
    [JsonPropertyName("impressora_comanda1")] public string? ImpressoraComanda1 { get; set; }
    [JsonPropertyName("impressora_comanda2")] public string? ImpressoraComanda2 { get; set; }
    [JsonPropertyName("tam_unico")] public bool TamanhoUnico { get; set; }
    [JsonPropertyName("fracionado")] public bool Fracionado { get; set; }
    [JsonPropertyName("tipo_de_venda")] public string? TipoDeVenda { get; set; }
    [JsonPropertyName("obs_na_venda")] public bool ObsNaVenda { get; set; }
    [JsonPropertyName("forma_de_venda")] public string? FormaDeVenda { get; set; }
    [JsonPropertyName("taxa_de_viagem")] public float TaxaDeViagem { get; set; }
    [JsonPropertyName("desconto")] public float Desconto { get; set; }
    [JsonPropertyName("validade")] public int validade { get; set; }
    [JsonPropertyName("acumula_quanto")] public int AcumulaQuanto { get; set; }
    [JsonPropertyName("quantidade_de_pontos_para_resgatar")] public int QuantidadeDePontosParaResgatar { get; set; }
    [JsonPropertyName("cardapio_dia")] public bool CardapioDoDia { get; set; }
    [JsonPropertyName("qtd_base")] public int QtdBase { get; set; }
    [JsonPropertyName("qtd_guarnicao")] public int QtdGuarnicao { get; set; }
    [JsonPropertyName("qtd_carnes")] public int QtdCarnes { get; set; }
    [JsonPropertyName("item_resgatavel")] public bool ItemResgatavel { get; set; }
    [JsonPropertyName("oculta_tablet")] public bool OcultaTablet { get; set; }
    [JsonPropertyName("ultiliza_produto_balanca")] public bool UltilizaProdutoNaBalanca { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }
    [JsonPropertyName("precos")] public List<Preco> Precos { get; set; } = new List<Preco>();
    [JsonPropertyName("grupo")] public ClsGrupo Grupo { get; set; } = new ClsGrupo();
    [JsonPropertyName("csosn")] public string? csosn { get; set; }




    private string? _CST;

    [JsonPropertyName("cst")]
    public string? CST
    {
        get => _CST;
        set
        {
            _CST = value;  
        }
    }







    private CSOSN? _csosnSelected;
    [JsonIgnore]
    public CSOSN? CSOSNSelected
    {
        get => _csosnSelected;
        set
        {
            _csosnSelected = value;
            csosn = value?.Id.ToString();
        }
    }

    private CST? _cstSelected;
    [JsonIgnore]
    public CST? CSTSelected
    {
        get => _cstSelected;
        set
        {
            _cstSelected = value;
            CST = value?.Id.ToString();
        }
    }

}

public class Preco
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("DescricaoTamanho")] public string? DescricaoDoTamanho { get; set; }
    [JsonPropertyName("CustosInsumo")] public float? CustosDoInsumo { get; set; }
    [JsonPropertyName("CustoReal")] public float? CustoReal { get; set; }
    [JsonPropertyName("PrecoSujetido")] public float? PrecoSujetido { get; set; }
    [JsonPropertyName("PorcentagemDeLucro")] public float? PorcentagemDeLucro { get; set; }
    [JsonPropertyName("Valor")] public double Valor { get; set; }
}

public class Categoria
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string? Descricao { get; set; }
}


public class CSOSN
{
    public int Id { get; set; }
    public string? Descricao { get; set; }

    // Lista fixa de CSOSN
    public static List<CSOSN> Lista => new()
    {
        new CSOSN { Id = 101, Descricao = "Tributada pelo Simples Nacional COM permissão de crédito" },
        new CSOSN { Id = 102, Descricao = "Tributada pelo Simples Nacional SEM permissão de crédito" },
        new CSOSN { Id = 103, Descricao = "Isenção do ICMS no Simples Nacional para faixa de receita bruta" },
        new CSOSN { Id = 201, Descricao = "Trib. Simples Nac. COM permissão de créd. e cobr. do ICMS por ST" },
        new CSOSN { Id = 202, Descricao = "Trib. Simples Nac. SEM permissão de créd. e cobr. do ICMS por ST" },
        new CSOSN { Id = 203, Descricao = "Isenção do ICMS no S.N. p/ faixa de rec. br. e cobr. do ICMS por ST" },
        new CSOSN { Id = 300, Descricao = "Imune" },
        new CSOSN { Id = 400, Descricao = "Não tributada pelo Simples Nacional" },
        new CSOSN { Id = 500, Descricao = "ICMS cobrado anteriormente por Subs. Trib." },
        new CSOSN { Id = 900, Descricao = "Outras" }
    };
}


public class CST
{
    public int Id { get; set; }
    public string? Descricao { get; set; }

    // Lista fixa de CSTs
    public static List<CST> Lista = new()
    {
        new CST { Id = 0, Descricao = "Tributada Integralmente" },
        new CST { Id = 10, Descricao = "Tributada e com cobrança do ICMS por Subs. Trib." },
        new CST { Id = 20, Descricao = "Com Redução de Base de Cálculo" },
        new CST { Id = 30, Descricao = "Isenta ou Não Trib. e cobr. de ICMS por Subs. Trib." },
        new CST { Id = 40, Descricao = "Isenta" },
        new CST { Id = 41, Descricao = "Não Tributada" },
        new CST { Id = 50, Descricao = "Suspensão" },
        new CST { Id = 51, Descricao = "Diferimento" },
        new CST { Id = 60, Descricao = "ICMS cobrado anteriormente por Subs. Trib." },
        new CST { Id = 90, Descricao = "Outras" }
    };
}
