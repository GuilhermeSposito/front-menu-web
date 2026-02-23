using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class PollingIfoodDto
{
    [JsonPropertyName("id")]public string Id { get; set; } = string.Empty;
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("fullCode")] public string FullCode { get; set; } = string.Empty;
    [JsonPropertyName("orderId")] public string OrderId { get; set; } = string.Empty;
    [JsonPropertyName("merchantId")] public string MerchantId { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = string.Empty;
    [JsonPropertyName("metadata")] public MetadataDto metadata { get; set; } = new MetadataDto();

}

public class MetadataDto
{
    [JsonPropertyName("CLIENT_ID")]public string? CLIENT_ID { get; set; }
    [JsonPropertyName("ORIGIN")] public string? ORIGIN { get; set; }
    [JsonPropertyName("appName")] public string? AppName { get; set; }
    [JsonPropertyName("details")] public string? Details { get; set; }
    [JsonPropertyName("ownerName")] public string? OwnerName { get; set; }
    [JsonPropertyName("reason_code")] public string? Reason_code { get; set; }
    [JsonPropertyName("acceptCancellationReasons")] public List<string> AcceptCancellationReasons { get; set; } = new List<string>();

}
