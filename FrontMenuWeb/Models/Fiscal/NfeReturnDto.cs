using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Fiscal;

public class NfeReturnDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("PedidoCaixaId")] public int? PedidoCaixaId { get; set; } = null;
    [JsonPropertyName("NfTipo")]public int? NFTipo { get; set; } = 65; //65 para NFCe e 55 para NFe
    [JsonPropertyName("ChaveNf")] public string? ChaveNf { get; set; }
    [JsonPropertyName("CStat")] public int Cstat { get; set; }
    [JsonPropertyName("xmotivo")] public string? Xmotivo { get; set; }
    [JsonPropertyName("NmrProtocolo")] public string? NmrProtocolo { get; set; }
    [JsonPropertyName("NmrDaNf")] public int NmrDaNf { get; set; }
    [JsonPropertyName("xml_distri")] public string? XmlStringField { get; set; }
}
