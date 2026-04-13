using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class InformacoesParaAutenticarEmpresaIfoodDto
{
    [JsonPropertyName("CodigoDeAutorizacaoEnviadoPeloIfood")] public string CodigoDeAutorizacaoEnviadoPeloIfood { get; set; } = string.Empty;
    [JsonPropertyName("VerificadorDoCodigo")] public string VerificadorDoCodigo { get; set; } = string.Empty;
    [JsonPropertyName("MerchantIdIfood")] public string MerchantIdIfood { get; set; } = string.Empty;
    [JsonPropertyName("NomeEmpresa")] public string NomeEmpresa { get; set; } = string.Empty;

}
