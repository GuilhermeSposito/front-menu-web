using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Produtos;

public class ClsAliquota
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = String.Empty;
    [JsonPropertyName("valor")] public decimal Valor { get; set; } = 0;
    [JsonPropertyName("confins")] public decimal Confis { get; set; } = 0;
    [JsonPropertyName("icms")] public decimal Icms { get; set; } = 0;
    [JsonPropertyName("pis")] public decimal Pis { get; set; } = 0;
    [JsonPropertyName("credIcms")] public decimal CredIcms { get; set; } = 0;
    [JsonIgnore]public int QtdDeProdutosRelacionados { get; set; } = 0;
}
