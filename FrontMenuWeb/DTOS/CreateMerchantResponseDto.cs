using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class CreateMerchantResponseDto
{
    [JsonPropertyName("status")] public int Status { get; set; }
    [JsonPropertyName("sucess")] public bool Sucess { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}
