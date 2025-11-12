using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class AbreCaixaDto
{
    [JsonPropertyName("ValorCaixaInicial")] public float ValorInicial { get; set; }
    [JsonPropertyName("DataAbertura")] public DateTime DataDeAbertura { get; set; } = DateTime.Now;
    [JsonPropertyName("Funcionario_id")] public int FuncionarioId { get; set; }
}
