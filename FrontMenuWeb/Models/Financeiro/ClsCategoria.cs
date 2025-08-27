using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Financeiro;

public class ClsCategoria
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("SubsCategorias")] public List<ClsSubCategoria> SubsCategorias { get; set; } = new List<ClsSubCategoria>();
}

public class ClsSubCategoria
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("categoria_id")] public int CategoriaId { get; set; }
}