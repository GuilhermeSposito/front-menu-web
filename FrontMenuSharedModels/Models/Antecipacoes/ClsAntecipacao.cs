using System.Text.Json.Serialization;
using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Merchant;

namespace FrontMenuWeb.Models.Antecipacoes;

public class ClsAntecipacao
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("mesa")] public ClsMesasEComandas? Mesa { get; set; }
    [JsonPropertyName("ValorAntecipacao")] public float ValorAntecipacao { get; set; }
    [JsonPropertyName("NomeClienteAntecipacao")] public string? NomeClienteAntecipacao { get; set; }
    [JsonPropertyName("FormaDeRecebimento")] public ClsFormaDeRecebimento? FormaDeRecebimento { get; set; }
    [JsonPropertyName("Fechado")] public bool Fechado { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime CriadoEm { get; set; }
}
