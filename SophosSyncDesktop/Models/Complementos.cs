using System.Text.Json.Serialization;

namespace SophosSyncDesktop.Models;

public class ClsGrupoDeComplemento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("grupoInfo")] public string? GrupoInfo { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } = true;
    [JsonPropertyName("ComplementosDosGrupos")] public List<ClsComplementoDoGrupo> Complementos { get; set; } = new List<ClsComplementoDoGrupo>();

    [JsonIgnore] public int QtdMin { get; set; } = 0; // usado para controle de relação com produto
    [JsonIgnore] public int QtdMax { get; set; } = 0; // usado para controle de relação com produto


    // controle de UI
    public bool Expanded { get; set; }
}

public class ClsComplementoDoGrupo
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Complemento")] public ClsComplemento Complemento { get; set; } = new ClsComplemento();
    [JsonPropertyName("Grupo")] public ClsGrupoDeComplemento Grupo { get; set; } = new ClsGrupoDeComplemento();

}

public class ClsGruposDeComplementosDoProduto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("Grupo")] public ClsGrupoDeComplemento Grupo { get; set; } = new ClsGrupoDeComplemento();
    [JsonPropertyName("ProdutoId")] public string ProdutoId { get; set; } = string.Empty;
    [JsonPropertyName("GrupoId")] public int GrupoId { get; set; }
    [JsonPropertyName("QtdMin")] public int QtdMin { get; set; }
    [JsonPropertyName("QtdMax")] public int QtdMax { get; set; }
}

public class ClsComplemento
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("ComplementoInfo")] public string? ComplementoInfo { get; set; }
    [JsonPropertyName("valor")] public float Valor { get; set; }
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } = true;
    [JsonPropertyName("EProduto")] public bool EProduto { get; set; } = false;
    [JsonPropertyName("produto_id")] public string? ProdutoId { get; set; }
    [JsonPropertyName("Produto")] public ClsProduto? Produto { get; set; }
    [JsonPropertyName("grupos")] public List<int> GruposIds { get; set; } = new List<int>();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)][JsonPropertyName("ComplementosDosGrupos")] public List<ClsComplementoDoGrupo> Grupos { get; set; } = new();
}


