using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class DtoEstastisticaPedidos
{
    [JsonPropertyName("TotalDeProdutosVendidos")] public double TotalDeProdutosVendidos { get; set; }
    [JsonPropertyName("Produtos")] public List<DtoEstastisticaPorProduto> EstatisticasPorProduto { get; set; } = new List<DtoEstastisticaPorProduto>();

}

public class DtoEstastisticaPorProduto
{
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Quantidade")] public double Quantidade { get; set; }
    [JsonPropertyName("ValorTotalDasVendas")] public double ValorTotalDasVendas { get; set; }
    [JsonPropertyName("Porcentagem")] public double Porcentagem { get; set; }
}
