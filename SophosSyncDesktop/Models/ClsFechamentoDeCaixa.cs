using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SophosSyncDesktop.Models;

public class ClsFechamentoDeCaixa
{
    [JsonPropertyName("ValorDeAbertura")] public float ValorDeAbertura { get; set; }
    [JsonPropertyName("Faltou")] public float Faltou { get; set; }
    [JsonPropertyName("ValorTotalEmVendas")] public float ValorTotalEmVendas { get; set; }
    [JsonPropertyName("TotalTaxaEntrega")] public float TotalTaxaEntrega { get; set; }
    [JsonPropertyName("TotalDeArescimos")] public float TotalDeArescimos { get; set; }
    [JsonPropertyName("TotalEmDescontos")] public float TotalEmDescontos { get; set; }
    [JsonPropertyName("TotalEmIncentivos")] public float TotalEmIncentivos { get; set; }
    [JsonPropertyName("TiketMedio")] public float TiketMedio { get; set; }
    [JsonPropertyName("recebimentosPorTipo")] public Dictionary<string, float>? RecebimentosPorTipo { get; set; }
    [JsonPropertyName("trocos")] public float Trocos { get; set; }
    [JsonPropertyName("sangrias")] public float Sangrias { get; set; }
    [JsonPropertyName("suprimentos")] public float Suprimentos { get; set; }
    [JsonPropertyName("ValorEsperadoEmDinheiro")] public float ValorEsperadoEmDinheiro { get; set; }
    [JsonPropertyName("TotalEmDinheiro")] public float TotalEmDinheiro { get; set; }
    [JsonPropertyName("TotalEmCartoes")] public float TotalEmCartoes { get; set; }
}
