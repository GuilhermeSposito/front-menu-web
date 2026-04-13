using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class CheckPermissionResponse
{
    [JsonPropertyName("allowed")]
    public bool Allowed { get; set; }
}
