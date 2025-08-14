using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class ClsMetodosDePagMerchant
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("ValorTotalEmPags")] public float ValorTotalEmPags { get; set; } = 0;
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } = true;
}
