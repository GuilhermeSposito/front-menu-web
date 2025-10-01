using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsMesasEComandas
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("CodigoExterno")] public int CodigoExterno { get; set; }
    [JsonPropertyName("Bloqueado")] public bool Bloqueado { get; set; }
    [JsonPropertyName("Reservado")] public bool Reservado { get; set; }
    [JsonPropertyName("InicioDoIntervalo")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? InicioDoIntervalo { get; set; } = 0;
    [JsonPropertyName("FimDoIntervalo")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? FimDoIntervalo { get; set; } = 0;
}
