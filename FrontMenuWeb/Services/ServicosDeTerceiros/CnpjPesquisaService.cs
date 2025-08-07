using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services.ServicosDeTerceiros;

public class CnpjPesquisaService
{
    private readonly HttpClient _http;

    public CnpjPesquisaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ClsCnpjInfoPesquisa?> ConsultarCNPJAsync(string cnpj)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ClsCnpjInfoPesquisa>($"{cnpj}");
            return response;
        }
        catch
        {
            return null;
        }
    }
}


public class Qsa
{
    [JsonPropertyName("pais")]
    public string? Pais { get; set; }

    [JsonPropertyName("nome_socio")]
    public string NomeSocio { get; set; } = string.Empty;

    [JsonPropertyName("codigo_pais")]
    public string? CodigoPais { get; set; }

    [JsonPropertyName("faixa_etaria")]
    public string FaixaEtaria { get; set; } = string.Empty;

    [JsonPropertyName("cnpj_cpf_do_socio")]
    public string CnpjCpfDoSocio { get; set; } = string.Empty;

    [JsonPropertyName("qualificacao_socio")]
    public string QualificacaoSocio { get; set; } = string.Empty;

    [JsonPropertyName("codigo_faixa_etaria")]
    public int CodigoFaixaEtaria { get; set; }

    [JsonPropertyName("data_entrada_sociedade")]
    public string DataEntradaSociedade { get; set; } = string.Empty;

    [JsonPropertyName("identificador_de_socio")]
    public int IdentificadorDeSocio { get; set; }

    [JsonPropertyName("cpf_representante_legal")]
    public string CpfRepresentanteLegal { get; set; } = string.Empty;

    [JsonPropertyName("nome_representante_legal")]
    public string NomeRepresentanteLegal { get; set; } = string.Empty;

    [JsonPropertyName("codigo_qualificacao_socio")]
    public int CodigoQualificacaoSocio { get; set; }

    [JsonPropertyName("qualificacao_representante_legal")]
    public string QualificacaoRepresentanteLegal { get; set; } = string.Empty;

    [JsonPropertyName("codigo_qualificacao_representante_legal")]
    public int CodigoQualificacaoRepresentanteLegal { get; set; }
}

public class RegimeTributario
{
    [JsonPropertyName("ano")]
    public int Ano { get; set; }

    [JsonPropertyName("cnpj_da_scp")]
    public string? CnpjDaScp { get; set; }

    [JsonPropertyName("forma_de_tributacao")]
    public string FormaDeTributacao { get; set; } = string.Empty;

    [JsonPropertyName("quantidade_de_escrituracoes")]
    public int QuantidadeDeEscrituracoes { get; set; }
}

public class CnaeSecundario
{
    [JsonPropertyName("codigo")]
    public int Codigo { get; set; }

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;
}

public class ClsCnpjInfoPesquisa
{
    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("qsa")]
    public List<Qsa> Qsa { get; set; } = new();

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("porte")]
    public string Porte { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; } = string.Empty;

    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("ddd_fax")]
    public string DddFax { get; set; } = string.Empty;

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("logradouro")]
    public string Logradouro { get; set; } = string.Empty;

    [JsonPropertyName("cnae_fiscal")]
    public long CnaeFiscal { get; set; }

    [JsonPropertyName("complemento")]
    public string Complemento { get; set; } = string.Empty;

    [JsonPropertyName("codigo_porte")]
    public int CodigoPorte { get; set; }

    [JsonPropertyName("razao_social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [JsonPropertyName("nome_fantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [JsonPropertyName("capital_social")]
    public decimal CapitalSocial { get; set; }

    [JsonPropertyName("ddd_telefone_1")]
    public string DddTelefone1 { get; set; } = string.Empty;

    [JsonPropertyName("ddd_telefone_2")]
    public string DddTelefone2 { get; set; } = string.Empty;

    [JsonPropertyName("opcao_pelo_mei")]
    public bool OpcaoPeloMei { get; set; }

    [JsonPropertyName("codigo_municipio")]
    public int CodigoMunicipio { get; set; }

    [JsonPropertyName("cnaes_secundarios")]
    public List<CnaeSecundario> CnaesSecundarios { get; set; } = new();

    [JsonPropertyName("natureza_juridica")]
    public string NaturezaJuridica { get; set; } = string.Empty;

    [JsonPropertyName("regime_tributario")]
    public List<RegimeTributario> RegimeTributario { get; set; } = new();

    [JsonPropertyName("situacao_especial")]
    public string SituacaoEspecial { get; set; } = string.Empty;

    [JsonPropertyName("opcao_pelo_simples")]
    public bool OpcaoPeloSimples { get; set; }

    [JsonPropertyName("situacao_cadastral")]
    public int SituacaoCadastral { get; set; }

    [JsonPropertyName("data_opcao_pelo_mei")]
    public string? DataOpcaoPeloMei { get; set; }

    [JsonPropertyName("cnae_fiscal_descricao")]
    public string CnaeFiscalDescricao { get; set; } = string.Empty;

    [JsonPropertyName("codigo_municipio_ibge")]
    public int CodigoMunicipioIbge { get; set; }

    [JsonPropertyName("data_inicio_atividade")]
    public string? DataInicioAtividade { get; set; }

    [JsonPropertyName("data_situacao_especial")]
    public string? DataSituacaoEspecial { get; set; }

    [JsonPropertyName("data_opcao_pelo_simples")]
    public string? DataOpcaoPeloSimples { get; set; }

    [JsonPropertyName("data_situacao_cadastral")]
    public string? DataSituacaoCadastral { get; set; }

    [JsonPropertyName("nome_cidade_no_exterior")]
    public string NomeCidadeNoExterior { get; set; } = string.Empty;

    [JsonPropertyName("codigo_natureza_juridica")]
    public int CodigoNaturezaJuridica { get; set; }

    [JsonPropertyName("data_exclusao_do_simples")]
    public string? DataExclusaoDoSimples { get; set; }

    [JsonPropertyName("motivo_situacao_cadastral")]
    public int MotivoSituacaoCadastral { get; set; }

    [JsonPropertyName("ente_federativo_responsavel")]
    public string EnteFederativoResponsavel { get; set; } = string.Empty;

    [JsonPropertyName("identificador_matriz_filial")]
    public int IdentificadorMatrizFilial { get; set; }

    [JsonPropertyName("qualificacao_do_responsavel")]
    public int QualificacaoDoResponsavel { get; set; }

    [JsonPropertyName("descricao_situacao_cadastral")]
    public string DescricaoSituacaoCadastral { get; set; } = string.Empty;

    [JsonPropertyName("descricao_tipo_de_logradouro")]
    public string DescricaoTipoDeLogradouro { get; set; } = string.Empty;

    [JsonPropertyName("descricao_motivo_situacao_cadastral")]
    public string DescricaoMotivoSituacaoCadastral { get; set; } = string.Empty;

    [JsonPropertyName("descricao_identificador_matriz_filial")]
    public string DescricaoIdentificadorMatrizFilial { get; set; } = string.Empty;
}

