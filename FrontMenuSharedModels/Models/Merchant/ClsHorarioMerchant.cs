using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsHorarioMerchant
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("dia")] public string Dia { get; set; } = string.Empty;
    [JsonPropertyName("horaInicio")] public DateTime HoraInicio { get; set; }
    [JsonPropertyName("horaFim")] public DateTime HoraFim { get; set; }
}

public class CriarHorarioMerchantDto
{
    [JsonPropertyName("dia")] public string Dia { get; set; } = string.Empty;
    [JsonPropertyName("hora_inicio")] public string HoraInicio { get; set; } = string.Empty;
    [JsonPropertyName("hora_fim")] public string HoraFim { get; set; } = string.Empty;
}

public class AtualizarHorarioMerchantDto
{
    [JsonPropertyName("dia")] public string? Dia { get; set; }
    [JsonPropertyName("hora_inicio")] public string? HoraInicio { get; set; }
    [JsonPropertyName("hora_fim")] public string? HoraFim { get; set; }
}
