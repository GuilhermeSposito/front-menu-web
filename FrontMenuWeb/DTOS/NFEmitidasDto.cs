using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class NFEmitidasDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("NfTipo")] public int NfTipo { get; set; }
    [JsonPropertyName("NmrNf")] public int NmrNf { get; set; }
    [JsonPropertyName("NmrProtocolo")] public string? NmrProtocolo { get; set; }
    [JsonPropertyName("ChaveNf")] public string? ChaveNf { get; set; }
    [JsonPropertyName("CStat")] public int CStat { get; set; }
    [JsonPropertyName("xmotivo")] public string? XMotivo { get; set; }
    [JsonPropertyName("xml_distri")] public string? XmlDistribuicao { get; set; }
    [JsonPropertyName("CriadoEm")] public DateTime? CriadoEm { get; set; }
    [JsonPropertyName("ValorTotalDaNf")] public double ValorTotalDaNf { get; set; }
    [JsonPropertyName("ValorTotalDosProdutos")] public double ValorTotalDosProdutos { get; set; }
    [JsonPropertyName("ValorTotalDosTributos")] public double ValorTotalDosTributos { get; set; }
}