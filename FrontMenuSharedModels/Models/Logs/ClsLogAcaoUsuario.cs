using FrontMenuWeb.Models.Merchant;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Logs;

public class ClsLogAcaoUsuario
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("merchant")] public ClsMerchant? Merchant { get; set; }
    [JsonPropertyName("acaoDescricao")] public string AcaoDescricao { get; set; } = string.Empty;
    [JsonPropertyName("horarioDaAcao")] public DateTime HorarioDaAcao { get; set; }
    [JsonPropertyName("funcionario")] public ClsFuncionario? Funcionario { get; set; }
    [JsonPropertyName("valorQueEstava")] public decimal? ValorQueEstava { get; set; }
    [JsonPropertyName("tipoAcao")] public string? TipoAcao { get; set; }
    [JsonPropertyName("entidadeAfetada")] public string? EntidadeAfetada { get; set; }
    [JsonPropertyName("idEntidadeAfetada")] public string? IdEntidadeAfetada { get; set; }
    [JsonPropertyName("valorNovo")] public decimal? ValorNovo { get; set; }
    [JsonPropertyName("ipAddress")] public string? IpAddress { get; set; }
}
