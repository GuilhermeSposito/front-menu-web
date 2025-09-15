using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Produtos;

public class ClsGrupoDeComplemento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("grupoInfo")] public string? GrupoInfo { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }
    [JsonPropertyName("ComplementosDosGrupos")] public List<ClsComplementoDoGrupo> Complementos { get; set; } = new List<ClsComplementoDoGrupo>();



    // controle de UI
    public bool Expanded { get; set; }
}

public class ClsComplementoDoGrupo
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Complemento")] public ClsComplemento Complemento { get; set; } = new ClsComplemento();
    [JsonPropertyName("Grupo")] public ClsGrupoDeComplemento Grupo { get; set; } = new ClsGrupoDeComplemento();

}

public class ClsComplemento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("ComplementoInfo")] public string? ComplementoInfo { get; set; }
    [JsonPropertyName("valor")] public float Valor { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; }
    [JsonPropertyName("ComplementosDosGrupos")] public List<ClsComplementoDoGrupo> Grupos { get; set; } = new();
}


