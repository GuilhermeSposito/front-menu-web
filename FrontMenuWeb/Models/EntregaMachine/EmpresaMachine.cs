using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.EntregaMachine;

public class EmpresaMachine
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("NomeEmpresa")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("EmailEmpresaIntegrada")] public string EmailEmpresaIntegrada { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("TokenApiEntrega")] public string TokenApiEntrega { get; set; } = string.Empty;
    [JsonPropertyName("TipoPagamento")] public string TipoPagamento { get; set; } = string.Empty;
    [JsonPropertyName("RetornAutomatico")] public bool RetornAutomatico { get; set; }
    [JsonPropertyName("CodEmpresa")] public int CodEmpresa { get; set; } = 3;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("SenhaEmpresaIntegrada")] public string? SenhaEmpresaIntegrada { get; set; }

}
