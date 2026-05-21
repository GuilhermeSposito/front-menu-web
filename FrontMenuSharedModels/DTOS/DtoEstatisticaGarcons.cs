using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class DtoEstatisticaGarcons
{
    [JsonPropertyName("TotalDeVendas")] public double TotalDeVendas { get; set; }
    [JsonPropertyName("TotalItensVendidos")] public double TotalItensVendidos { get; set; }
    [JsonPropertyName("TaxaDeServicoPorcentagem")] public double TaxaDeServicoPorcentagem { get; set; }
    [JsonPropertyName("Garcons")] public List<DtoGarconEstatistica> Garcons { get; set; } = new();
}

public class DtoGarconEstatistica
{
    [JsonPropertyName("Garcon")] public DtoGarconInfoEstatistica Garcon { get; set; } = new();
    [JsonPropertyName("ValorDeVendas")] public double ValorDeVendas { get; set; }
    [JsonPropertyName("ValorTaxaDeServico")] public double ValorTaxaDeServico { get; set; }
    [JsonPropertyName("ValorEmPorcentagem")] public double ValorEmPorcentagem { get; set; }
    [JsonPropertyName("QtdItensVendidos")] public double QtdItensVendidos { get; set; }
    [JsonPropertyName("QtdLinhasDePedido")] public double QtdLinhasDePedido { get; set; }
}

public class DtoGarconInfoEstatistica
{
    [JsonPropertyName("Id")] public int Id { get; set; }
    [JsonPropertyName("Nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("NomeUsuario")] public string NomeUsuario { get; set; } = string.Empty;
}
