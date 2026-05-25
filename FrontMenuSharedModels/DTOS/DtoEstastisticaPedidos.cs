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
    [JsonPropertyName("QuantidadeVendida")] public double Quantidade { get; set; }
    [JsonPropertyName("ValorTotalDasVendas")] public double ValorTotalDasVendas { get; set; }
    [JsonPropertyName("Porcentagem")] public double Porcentagem { get; set; }
}

public class DtoHoraVendas
{
    [JsonPropertyName("Hora")] public int Hora { get; set; }
    [JsonPropertyName("TotalPedidos")] public double TotalPedidos { get; set; }
    [JsonPropertyName("ValorTotal")] public double ValorTotal { get; set; }
}

public class DtoHoraItens
{
    [JsonPropertyName("Hora")] public int Hora { get; set; }
    [JsonPropertyName("Quantidade")] public double Quantidade { get; set; }
    [JsonPropertyName("ValorTotal")] public double ValorTotal { get; set; }
}

public class DtoProdutoComHorario
{
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("TotalVendido")] public double TotalVendido { get; set; }
    [JsonPropertyName("ValorTotal")] public double ValorTotal { get; set; }
    [JsonPropertyName("Porcentagem")] public double Porcentagem { get; set; }
    [JsonPropertyName("HoraDePico")] public int HoraDePico { get; set; }
}

public class RetornoEstatisticasCaixaAtual
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("message")] public List<string> Message { get; set; } = new();
    [JsonPropertyName("data")] public DtoEstatisticaItensCaixaAtual? Data { get; set; }
}

public class DtoEstatisticaItensCaixaAtual
{
    [JsonPropertyName("TotalDeItensVendidos")] public double TotalDeItensVendidos { get; set; }
    [JsonPropertyName("ValorTotalVendido")] public double ValorTotalVendido { get; set; }
    [JsonPropertyName("TotalDeProdutosDistintos")] public int TotalDeProdutosDistintos { get; set; }
    [JsonPropertyName("Itens")] public List<DtoItemVendidoCaixaAtual> Itens { get; set; } = new();
}

public class DtoItemVendidoCaixaAtual
{
    [JsonPropertyName("Ranking")] public int Ranking { get; set; }
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("QuantidadeVendida")] public double QuantidadeVendida { get; set; }
    [JsonPropertyName("ValorTotalDasVendas")] public double ValorTotalDasVendas { get; set; }
    [JsonPropertyName("PrecoUnitarioMedio")] public double PrecoUnitarioMedio { get; set; }
    [JsonPropertyName("PorcentagemDasVendas")] public double PorcentagemDasVendas { get; set; }
    [JsonPropertyName("HorarioDeMaiorIncidencia")] public int HorarioDeMaiorIncidencia { get; set; }
}
