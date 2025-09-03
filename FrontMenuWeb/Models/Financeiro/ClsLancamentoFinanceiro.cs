using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using OneOf.Types;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class ClsLancamentoFinanceiro
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Valor")] public float Valor { get; set; }
    [JsonPropertyName("DataVencimento")] public DateTime? DataDeVencimento { get; set; } = DateTime.Today;
    [JsonPropertyName("DataEmissao")] public DateTime? DataDeEmissao { get; set; } = DateTime.Today;
    [JsonPropertyName("DataPagamento")] public DateTime? DataDePagamento { get; set; } = DateTime.Today;
    [JsonPropertyName("Pago")] public bool Pago { get; set; } = false;
    [JsonPropertyName("Repete")] public bool Repete { get; set; } = false;
    [JsonPropertyName("Parcelado")] public bool Parcelado { get; set; } = false;
    [JsonPropertyName("QtdDeParcelas")] public int QtdParcelas { get; set; } = 0;
    [JsonPropertyName("Identificador")] public string Identificado { get; set; } = string.Empty;
    [JsonPropertyName("EDespesaFixa")] public bool EDespesaFixa { get; set; } = false;
    [JsonPropertyName("Obs")] public string Obs { get; set; } = string.Empty;
    [JsonPropertyName("QtdMesesDeDespesaFixa")] public int QtdMesesDeDespesaFixa { get; set; } = 0;
    [JsonPropertyName("QtdAtualDaParcela")] public int QtdAtualDaParcela { get; set; } = 0;


    //Chaves estrangeiras
    [JsonPropertyName("tipoDeLancFinanceiro_id")] public int TipoDeLancFinanceiroID { get; set; }
    [JsonPropertyName("conta_id")] public int ContaId { get; set; }
    [JsonPropertyName("categoria_id")] public int? CategoriaID { get; set; }
    [JsonPropertyName("sub_categoria_id")] public int? SubCategoriaID { get; set; }
    [JsonPropertyName("metodo_de_pagamento_id")] public int MetodoDePagID { get; set; }
    [JsonPropertyName("pessoa_id")] public int? PessoaID { get; set; }



    //Tabelas de relacionamento
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("TipoDeLancamento")] public ClsTipoDeLancamento TipoDeLancamento { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("Conta")] public ClsConta Conta { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("Categoria")] public ClsCategoria? Categoria { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("SubCategoria")] public ClsSubCategoria? SubCategoria { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("MetodoDePagamento")] public ClsMetodosDePagMerchant MetodoDePagamento { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("Pessoa")] public ClsPessoas? Pessoa { get; set; }


}

public class ClsTipoDeLancamento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("fator")] public int fator { get; set; } = 1; // 1 para receita, -1 para despesa

}


public class ClsFiltros
{
    public int Id { get; set; } = 0;
    public string NomeDoFiltro { get; set; } = string.Empty;
}

public static class Filtros
{
    public static List<ClsFiltros> filtros = new List<ClsFiltros>
    {
        new ClsFiltros { Id = 0, NomeDoFiltro = "Descrição" },
        new ClsFiltros { Id = 1, NomeDoFiltro = "Periodo" },
        new ClsFiltros { Id = 2, NomeDoFiltro = "Situação" },
        new ClsFiltros { Id = 4, NomeDoFiltro = "Data de Emissão" },
        new ClsFiltros { Id = 5, NomeDoFiltro = "Fornecedor" },
        new ClsFiltros { Id = 6, NomeDoFiltro = "Data de Pagamento" },
        new ClsFiltros { Id = 7, NomeDoFiltro = "Data de Vencimento" },
        new ClsFiltros { Id = 7, NomeDoFiltro = "Método de Pagamento" }
    };
}