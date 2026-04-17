using FrontMenuWeb.Models.Produtos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Vendas;

public class CaixaAbertoStatus
{
    [JsonPropertyName("CaixaId")] public int CaixaId { get; set; }
    [JsonPropertyName("Funcionario")] public string Funcionario { get; set; } = "Admin";
    [JsonPropertyName("DataAbertura")] public DateTime? DataAbertura { get; set; }
    [JsonPropertyName("ValorInicial")] public float ValorInicial { get; set; }
    [JsonPropertyName("ValorTotalEmVendas")] public float ValorTotalEmVendas { get; set; }
    [JsonPropertyName("QtdPedidos")] public ClsPercentualDePedidos? QtdPedidos { get; set; }
    [JsonPropertyName("TiketMedio")] public float TiketMedio { get; set; }
    [JsonPropertyName("RecebimentosPorTipo")] public Dictionary<string, float>? RecebimentosPorTipo { get; set; }
    [JsonPropertyName("Trocos")] public float Trocos { get; set; }
    [JsonPropertyName("Sangrias")] public float Sangrias { get; set; }
    [JsonPropertyName("Suprimentos")] public float Suprimentos { get; set; }
    [JsonPropertyName("ValorEsperado")] public float ValorEsperado { get; set; }
    [JsonPropertyName("ValorEsperadoEmDinheiro")] public float ValorEsperadoEmDinheiro { get; set; }
    [JsonPropertyName("PorcentagemPedidos")] public Dictionary<string, float>? PorcentagemPedidos { get; set; }
}
