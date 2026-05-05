using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class MerchantByInstanceDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("NomeFantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [JsonPropertyName("InstanceName")]
    public string? InstanceName { get; set; }
}

public class CriarWhatsAppMensagemDto
{
    [JsonPropertyName("merchantId")]
    public string MerchantId { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumberId")]
    public string PhoneNumberId { get; set; } = string.Empty;

    [JsonPropertyName("fromNumber")]
    public string FromNumber { get; set; } = string.Empty;

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("receivedAt")]
    public string ReceivedAt { get; set; } = string.Empty;

    [JsonPropertyName("autoReplySent")]
    public bool AutoReplySent { get; set; }
}

public class DataDeletionResponseDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("confirmation_code")]
    public string ConfirmationCode { get; set; } = string.Empty;
}

public class FacebookSignedPayloadDto
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; set; }

    [JsonPropertyName("issued_at")]
    public long IssuedAt { get; set; }
}
