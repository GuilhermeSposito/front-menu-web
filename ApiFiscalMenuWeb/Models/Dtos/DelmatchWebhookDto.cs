using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class DelmatchWebhookDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("empresa_id")] public int EmpresaId { get; set; }
    [JsonPropertyName("Status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("is_visualizado")] public bool IsVisualizado { get; set; }
    [JsonPropertyName("celular")] public string Celular { get; set; } = string.Empty;
    [JsonPropertyName("cliente_nome")] public string ClienteNome { get; set; } = string.Empty;
}
