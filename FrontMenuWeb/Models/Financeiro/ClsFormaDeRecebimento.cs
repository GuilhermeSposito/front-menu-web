using FrontMenuWeb.Models.Financeiro;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class ClsFormaDeRecebimento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("pagamentoOnline")] public bool PagamentoOnline { get; set; } = false;
    [JsonPropertyName("convenio")] public bool Convenio { get; set; } = false;
    [JsonPropertyName("qtd_dias_para_reembolso")] public int QtdDiasParaReembolso { get; set; } = 0;
    [JsonPropertyName("taxa")] public float Taxa { get; set; } = 0.0f;
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } = true;
    [JsonPropertyName("ContasDeFormaDeRecebimento")] public List<ClsDeContasDasFormasDeRecebimento> ListasDeContasDaForma { get; set; } = new List<ClsDeContasDasFormasDeRecebimento>();
    [JsonPropertyName("chave_pix")] public string? ChavePix { get; set; }
    [JsonPropertyName("contasIds")] public List<int> ContasIds { get; set; } = new List<int>();

}

public class ClsDeContasDasFormasDeRecebimento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("conta")] public ClsConta Conta { get; set; }

}

