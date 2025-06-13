using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Produtos;

public class ClsProduto
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("codigo_interno")] public string? CodigoInterno { get; set; }
    [JsonPropertyName("categoria_id")] public int CategoriaId { get; set; }
    [JsonPropertyName("grupo_id")] public int GrupoId { get; set; }
    [JsonPropertyName("descricao")] public string? Descricao { get; set; }
    [JsonPropertyName("ncm")] public string? NCM { get; set; }
    [JsonPropertyName("cest")] public string? CEST { get; set; }
    [JsonPropertyName("cst")] public string? CST { get; set; }
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
    [JsonPropertyName("aculta_tablet")] public bool AcumulaTablet { get; set; }
    [JsonPropertyName("ultiliza_produto_balanca")] public bool UltilizaProdutoNaBalanca { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }
    [JsonPropertyName("precos")] public List<Preco> Precos { get; set; } = new List<Preco>();
    [JsonPropertyName("grupo")] public ClsGrupo Grupo { get; set; } = new ClsGrupo();

}

public class  Preco
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("DescricaoTamanho")] public string? DescricaoDoTamanho { get; set; }
    [JsonPropertyName("valor")] public float Valor { get; set; }   
}