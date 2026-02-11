using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class QueryCaixasDto
{
    [JsonPropertyName("limit")] public int Limit { get; set; } = 10;
    [JsonPropertyName("page")] public int page { get; set; } = 1;
    [JsonPropertyName("DataFechadoEmInicio")] public DateTime? DataFechadoEmInicio { get; set; }
    [JsonPropertyName("DataFechadoEmFinal")] public DateTime? DataFechadoEmFinal { get; set; }


}
