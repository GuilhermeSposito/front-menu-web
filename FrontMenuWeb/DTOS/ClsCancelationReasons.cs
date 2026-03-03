using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class ClsCancelationReasons
{
    [JsonPropertyName("cancelCodeId")] public string? CancelCodeId { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }


}
