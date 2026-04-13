using FrontMenuWeb.Models.Merchant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FrontMenuSharedModels.Models.Sangria;

public class ClsSangria
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
    [JsonPropertyName("status")] public EnumSangriaStatus Status { get; set; }
    [JsonPropertyName("motivoCancelamento")] public string MotivoCancelamento { get; set; } = string.Empty;
    [JsonPropertyName("canceladoEm")] public DateTime? CanceladoEm { get; set; }
    [JsonPropertyName("criadoEm")] public DateTime CriadoEm { get; set; }
    [JsonPropertyName("canceladoPorFuncionario")] public ClsFuncionario? FuncionarioQueCancelou { get; set; }
    [JsonPropertyName("funcionario")] public ClsFuncionario? FuncionarioQueCriou { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnumSangriaStatus
{
    Cancelado,
    Ativo
}