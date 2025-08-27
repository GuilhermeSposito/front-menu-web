using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using OneOf.Types;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class ClsLancamentoFinanceiro
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Valor")] public float Valor { get; set; }
    [JsonPropertyName("DataVencimento")] public DateTime DataDeVencimento { get; set; }
    [JsonPropertyName("DataEmissao")] public DateTime DataDeEmissao { get; set; }
    [JsonPropertyName("DataPagamento")] public DateTime? DataDePagamento { get; set; }
    [JsonPropertyName("Pago")] public bool Pago { get; set; } = false;
    [JsonPropertyName("Repete")] public bool Repete { get; set; } = false;
    [JsonPropertyName("Parcelado")] public bool Parcelado { get; set; } = false;
    [JsonPropertyName("QtdParcela")] public int QtdParcelas { get; set; } = 0;
    [JsonPropertyName("Identificador")] public string Identificado { get; set; } = string.Empty;
    [JsonPropertyName("EDespesaFixa")] public bool EDespesaFixa { get; set; } = false;
    [JsonPropertyName("Obs")] public string Obs { get; set; } = string.Empty;

    //Tabelas de relacionamento
    [JsonPropertyName("TipoDeLancamento")] public ClsTipoDeLancamento TipoDeLancamento { get; set; } = new();
    [JsonPropertyName("Conta")] public ClsConta Conta { get; set; } = new();
    [JsonPropertyName("Categoria")] public ClsCategoria? Categoria { get; set; }
    [JsonPropertyName("SubCategoria")] public ClsSubCategoria? SubCategoria { get; set; }
    [JsonPropertyName("MetodoDePagamento")] public ClsMetodosDePagMerchant MetodoDePagamento { get; set; } = new();
    [JsonPropertyName("Pessoa")] public ClsPessoas? Pessoa { get; set; }


}

public class ClsTipoDeLancamento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("fator")] public int fator { get; set; } = 1; // 1 para receita, -1 para despesa

}
