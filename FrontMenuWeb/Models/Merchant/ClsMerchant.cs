using FrontMenuWeb.Models.Produtos;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsMerchant
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("razaoSocial")] public string RazaoSocial { get; set; } = string.Empty;
    [JsonPropertyName("ImagemLogo")] public string ImagemLogo { get; set; } = string.Empty;
    [JsonPropertyName("NomeFantasia")] public string NomeFantasia { get; set; } = string.Empty;
    [JsonPropertyName("Grupos")] public List<ClsGrupo> Grupos { get; set; } = new List<ClsGrupo>();
    [JsonPropertyName("marcaDepartamento")] public string? MarcaDepartamento { get; set; } = string.Empty;
    [JsonPropertyName("legendaDoVoluma")] public string? LegendaDoVolume { get; set; } = string.Empty;
    [JsonPropertyName("ativo")] public bool Ativo { get; set; } 
    [JsonPropertyName("FuncionarioLogado")] public ClsFuncionario? FuncionarioLogado { get; set; } 


}
