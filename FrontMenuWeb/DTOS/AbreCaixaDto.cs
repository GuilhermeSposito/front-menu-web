using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class AbreCaixaDto
{
    [JsonPropertyName("ValorCaixaInicial")] public float ValorInicial { get; set; }
    [JsonPropertyName("DataAbertura")] public DateTime DataDeAbertura { get; set; } = DateTime.Now;
    [JsonPropertyName("Funcionario_id")] public int FuncionarioId { get; set; }
}

public class FechaCaixaDto
{
    [JsonPropertyName("ValorRecebimentosCredito")] public float ValorRecebimentosCredito { get; set; }
    [JsonPropertyName("ValorRecebimentosDebito")] public float ValorRecebimentosDebito { get; set; }
    [JsonPropertyName("ValorRecebimentosPix")] public float ValorRecebimentosPix { get; set; }
    [JsonPropertyName("ValorFinalEmDinheiro")] public float ValorFinalEmDinheiro { get; set; }
    [JsonPropertyName("Funcionario_id")] public int FuncionarioId { get; set; }
}