using FrontMenuWeb.Models.Merchant;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Caixa;

public class ClsSuprimento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty; // "Ativa" | "Cancelada"
    [JsonPropertyName("motivoCancelamento")] public string? MotivoCancelamento { get; set; }
    [JsonPropertyName("canceladoEm")] public DateTime? CanceladoEm { get; set; }
    [JsonPropertyName("criadoEm")] public DateTime CriadoEm { get; set; }
    [JsonPropertyName("funcionario")] public ClsFuncionario? Funcionario { get; set; }
    [JsonPropertyName("canceladoPorFuncionario")] public ClsFuncionario? CanceladoPorFuncionario { get; set; }
}
