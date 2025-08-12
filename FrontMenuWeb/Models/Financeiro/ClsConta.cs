using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class ClsConta
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("nomeConta")] public string NomeConta { get; set; } = string.Empty;
    [JsonPropertyName("codIconeBanco")] public int CodIconeDoBanco { get; set; } = 0;
    [JsonPropertyName("saldoInicial")] public float SaldoInicial { get; set; }
    [JsonPropertyName("dataInicial")] public DateTime DataInicial { get; set; } = DateTime.Now.ToLocalTime();
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } = true;
}


public class  RetornoApiContas
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("messages")] public List<string> Messages { get; set; } = new List<string>();
    [JsonPropertyName("data")] public Data Data { get; set; } = new Data();
}

public class Data
{
    [JsonPropertyName("Contas")] public List<ClsConta> Contas { get; set; } = new List<ClsConta>();
    [JsonPropertyName("Conta")] public ClsConta Conta { get; set; } = new ClsConta();

}