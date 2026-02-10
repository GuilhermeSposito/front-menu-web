using FrontMenuWeb.Models.Produtos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsMerchant
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("razaoSocial")] public string RazaoSocial { get; set; } = string.Empty;
    [JsonPropertyName("ImagemLogo")] public string? ImagemLogo { get; set; }
    [JsonPropertyName("NomeFantasia")] public string NomeFantasia { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("Grupos")] public List<ClsGrupo> Grupos { get; set; } = new List<ClsGrupo>();
    [JsonPropertyName("marcaDepartamento")] public string? MarcaDepartamento { get; set; } = string.Empty;
    [JsonPropertyName("legendaDoVoluma")] public string? LegendaDoVolume { get; set; } = string.Empty;
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }
    [JsonPropertyName("EmitindoNfeProd")] public bool EmitindoNfeProd { get; set; }
    [JsonPropertyName("FuncionarioLogado")] public ClsFuncionario? FuncionarioLogado { get; set; }
    [JsonPropertyName("CertificadoBase64")] public string? CertificadoBase64 { get; set; }
    [JsonPropertyName("SenhaCertificado")] public string? SenhaCertificado { get; set; }
    [JsonPropertyName("UltimoNmrSerieNFe")] public int UltimoNmrSerieNFe { get; set; }
    [JsonPropertyName("UltimoNmrSerieNFCe")] public int UltimoNmrSerieNFCe { get; set; }
    [JsonPropertyName("enderecos_merchant")] public List<EnderecoMerchant> EnderecosMerchant { get; set; } = new List<EnderecoMerchant>();
    [JsonPropertyName("documentos")] public List<DocumentosMerchant> Documentos { get; set; } = new List<DocumentosMerchant>();
    [JsonPropertyName("CrtMerchant")] public int CrtMerchant { get; set; }
    [JsonPropertyName("AliquotaPis")] public decimal AliquotaPis { get; set; }
    [JsonPropertyName("AliquotaCofins")] public decimal AliquotaCofins { get; set; }
    [JsonPropertyName("AliquotaCsll")] public decimal AliquotaCsll { get; set; }
    [JsonPropertyName("AliquotaIrpj")] public decimal AliquotaIrpj { get; set; }
    [JsonPropertyName("AliquotaIssqn")] public decimal AliquotaIssqn { get; set; }
    [JsonPropertyName("AliquotaIcms")] public decimal AliquotaIcms { get; set; }
    [JsonPropertyName("PCredSimplesNacional")] public decimal PCredSimplesNacional { get; set; }
    [JsonPropertyName("PosicaoDanfe")] public string PosicaoDanfe { get; set; } = "retrato";
    [JsonPropertyName("NcmDeServico")] public string? NcmDeServico { get; set; }
    [JsonPropertyName("CfopDeServico")] public string? CfopDeServico { get; set; }
    [JsonPropertyName("ImprimeComandasDelivery")] public bool ImprimeComandasDelivery { get; set; }
    [JsonPropertyName("ImprimeComandasBalcao")] public bool ImprimeComandasBalcao { get; set; }
    [JsonPropertyName("ImprimeComandasMesas")] public bool ImprimeComandasMesas { get; set; }
    [JsonPropertyName("ImprimeComandasSeparadaPorProdutos")] public bool ImprimeComandasSeparadaPorProdutos { get; set; }
    [JsonPropertyName("DestacaObsNaImpressao")] public bool DestacaObsNaImpressao { get; set; }
    [JsonPropertyName("ImprimeNomeNaComanda")] public bool ImprimeNomeNaComanda { get; set; }
    [JsonPropertyName("ImprimeEnderecoNaComanda")] public bool ImprimeEnderecoNaComanda { get; set; }
    [JsonPropertyName("ImprimeHorarioNaComanda")] public bool ImprimeHorarioNaComanda { get; set; }
    [JsonPropertyName("AceitaFiado")] public bool AceitaFiado { get; set; }
    [JsonPropertyName("EnviaPedidoAutTaxyMachine")] public bool EnviaPedidoAutTaxyMachine { get; set; }
    [JsonPropertyName("IntegraIfood")] public bool IntegraIfood { get; set; }
    [JsonPropertyName("ReplicaFechamentoParaOFinanceiro")] public bool ReplicaFechamentoParaOFinanceiro { get; set; }
    [JsonPropertyName("TempoDeEntregaEMmin")] public int TempoDeEntregaEMmin { get; set; }
    [JsonPropertyName("TempoDeRetiradaEMmin")] public int TempoDeRetiradaEMmin { get; set; }
    [JsonPropertyName("TempoDePreparacaoEmMin")] public int TempoDePreparacaoEmMin { get; set; }
    [JsonPropertyName("QtdViasDaComanda")] public int QtdViasDaComanda { get; set; }
    [JsonPropertyName("TokenApiEntrega")] public string? TokenApiEntrega { get; set; }
    [JsonPropertyName("ValorPorKm")] public float ValorPorKm { get; set; }
    [JsonPropertyName("KmMinimo")] public float KmMinimo { get; set; }
    [JsonPropertyName("ValorKmMinimo")] public float ValorKmMinimo { get; set; }
    [JsonPropertyName("EspacamentoNaImpressao")] public int EspacamentoNaImpressao { get; set; }
    [JsonPropertyName("TamFonteDetalhesPedido")] public int TamFonteDetalhesPedido { get; set; }
    [JsonPropertyName("TamFonteDescricaoItem")] public int TamFonteDescricaoItem { get; set; }
    [JsonPropertyName("TamFonteDescricaoItemNaComanda")] public int TamFonteDescricaoItemNaComanda { get; set; }
    [JsonPropertyName("TamFonteDescricaoComplemento")] public int TamFonteDescricaoComplemento { get; set; }
    [JsonPropertyName("TamFonteDescricaoComplementoNaComanda")] public int TamFonteDescricaoComplementoNaComanda { get; set; }
    [JsonPropertyName("TamFonteValorItem")] public int TamFonteValorItem { get; set; }
    [JsonPropertyName("TamFonteTempoEntregaEConta")] public int TamFonteTempoEntregaEConta { get; set; }
    [JsonPropertyName("TamFonteLegendaDosItens")] public int TamFonteLegendaDosItens { get; set; }
    [JsonPropertyName("TamFonteTotais")] public int TamFonteTotais { get; set; }
    [JsonPropertyName("TamFonteInfosPag")] public int TamFonteInfosPag { get; set; }
    [JsonPropertyName("TamFonteNomeClienteComanda")] public int TamFonteNomeClienteComanda { get; set; }
    [JsonPropertyName("UltilizaEntregaPorKm")] public bool UltilizaEntregaPorKm { get; set; }
    [JsonPropertyName("PerguntaImpressaoBalcao")] public bool PerguntaImpressaoBalcao { get; set; }
    [JsonPropertyName("PerguntaImpressaoDelivery")] public bool PerguntaImpressaoDelivery { get; set; }
    [JsonPropertyName("PerguntaImpressaoMesa")] public bool PerguntaImpressaoMesa { get; set; }
    [JsonPropertyName("PerguntaImpressaPdv")] public bool PerguntaImpressaPdv { get; set; }
    [JsonPropertyName("ControlaEstoque")] public bool ControlaEstoque { get; set; }
    [JsonPropertyName("UsaCodigoDeBarrasParAProximaEtapa")] public bool UsaCodigoDeBarrasParAProximaEtapa { get; set; }
    [JsonPropertyName("ImprimeHorarioLimiteNoPedido")] public bool ImprimeHorarioLimiteNoPedido { get; set; }
}




public class DocumentosMerchant
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("cnpj")] public string Cnpj { get; set; } = string.Empty;
    [JsonPropertyName("inscricaoEstadual")] public string IncricaoEstadual { get; set; } = string.Empty;
    [JsonPropertyName("inscricaoMunicipal")] public string InscricaoMunicipal { get; set; } = string.Empty;
    [JsonPropertyName("cnae")] public string Cnae { get; set; } = string.Empty;
    [JsonPropertyName("CSC")] public string? CSC { get; set; }
    [JsonPropertyName("IdCscToken")] public int? IdCscToken { get; set; } = 1;
}

public class EnderecoMerchant
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("cidade")] public CidadeMerchant Cidade { get; set; } = new CidadeMerchant();
    [JsonPropertyName("rua")] public string Rua { get; set; } = string.Empty;
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("bairro")] public string Bairro { get; set; } = string.Empty;
    [JsonPropertyName("cep")] public string Cep { get; set; } = string.Empty;
    [JsonPropertyName("uf")] public string Uf { get; set; } = string.Empty;
    [JsonPropertyName("cidade_id")] public int CidadeId { get; set; } = 1; //São Carlos

    [JsonIgnore] public bool AdicionandoEsseEndereco { get; set; } = false;

}

public class CidadeMerchant
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("numCidade")] public int NumCidade { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
}