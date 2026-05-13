using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.FichaTecnica;

public class ClsFichaTecnica
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("PrecoProduto")] public ClsPrecoProdutoFicha? PrecoProduto { get; set; }
    [JsonPropertyName("Itens")] public List<ClsItemFichaTecnica> Itens { get; set; } = new();
}

public class ClsPrecoProdutoFicha
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("DescricaoTamanho")] public string DescricaoTamanho { get; set; } = string.Empty;
}

public class ClsItemFichaTecnica
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ProdutoInsumo")] public ClsProdutoInsumoResumo? ProdutoInsumo { get; set; }
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; }
    [JsonPropertyName("UnidadeMedida")] public string UnidadeMedida { get; set; } = string.Empty;
}

public class ClsProdutoInsumoResumo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("descricao")] public string Descricao { get; set; } = string.Empty;
}

public class CreateFichaTecnicaRequest
{
    [JsonPropertyName("produtoId")] public string ProdutoId { get; set; } = string.Empty;
    [JsonPropertyName("precoProdutoId")] public string? PrecoProdutoId { get; set; }
}

public class CreateItemFichaTecnicaRequest
{
    [JsonPropertyName("produtoInsumoId")] public string ProdutoInsumoId { get; set; } = string.Empty;
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; }
    [JsonPropertyName("UnidadeMedida")] public string UnidadeMedida { get; set; } = string.Empty;
}

public class UpdateItemFichaTecnicaRequest
{
    [JsonPropertyName("Quantidade")] public float Quantidade { get; set; }
    [JsonPropertyName("UnidadeMedida")] public string UnidadeMedida { get; set; } = string.Empty;
}
