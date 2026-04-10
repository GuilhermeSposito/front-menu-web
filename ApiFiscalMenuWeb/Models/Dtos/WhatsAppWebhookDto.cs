using System.Text.Json;
using System.Text.Json.Serialization;

public class WhatsAppWebhookDto
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("entry")]
    public List<WhatsAppEntryDto>? Entry { get; set; }
}

public class WhatsAppEntryDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("changes")]
    public List<WhatsAppChangeDto>? Changes { get; set; }
}

public class WhatsAppChangeDto
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("value")]
    public WhatsAppValueDto? Value { get; set; }
}

public class WhatsAppValueDto
{
    [JsonPropertyName("messaging_product")]
    public string? MessagingProduct { get; set; }

    [JsonPropertyName("metadata")]
    public MetadataDto? Metadata { get; set; }

    [JsonPropertyName("contacts")]
    public List<ContactDto>? Contacts { get; set; }

    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }

    [JsonPropertyName("statuses")]
    public List<StatusDto>? Statuses { get; set; }

    [JsonPropertyName("errors")]
    public List<WebhookErrorDto>? Errors { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}

public class MetadataDto
{
    [JsonPropertyName("display_phone_number")]
    public string? DisplayPhoneNumber { get; set; }

    [JsonPropertyName("phone_number_id")]
    public string? PhoneNumberId { get; set; }
}

public class ContactDto
{
    [JsonPropertyName("profile")]
    public ProfileDto? Profile { get; set; }

    [JsonPropertyName("wa_id")]
    public string? WaId { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}

public class ProfileDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class MessageDto
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("from_user_id")]
    public string? FromUserId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("context")]
    public MessageContextDto? Context { get; set; }

    [JsonPropertyName("text")]
    public TextMessageDto? Text { get; set; }

    [JsonPropertyName("image")]
    public MediaMessageDto? Image { get; set; }

    [JsonPropertyName("document")]
    public DocumentMessageDto? Document { get; set; }

    [JsonPropertyName("audio")]
    public MediaMessageDto? Audio { get; set; }

    [JsonPropertyName("video")]
    public MediaMessageDto? Video { get; set; }

    [JsonPropertyName("sticker")]
    public MediaMessageDto? Sticker { get; set; }

    [JsonPropertyName("button")]
    public ButtonMessageDto? Button { get; set; }

    [JsonPropertyName("interactive")]
    public InteractiveMessageDto? Interactive { get; set; }

    [JsonPropertyName("location")]
    public LocationMessageDto? Location { get; set; }

    [JsonPropertyName("contacts")]
    public List<SharedContactDto>? Contacts { get; set; }

    [JsonPropertyName("reaction")]
    public ReactionMessageDto? Reaction { get; set; }

    [JsonPropertyName("referral")]
    public ReferralDto? Referral { get; set; }

    [JsonPropertyName("errors")]
    public List<WebhookErrorDto>? Errors { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}

public class MessageContextDto
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("forwarded")]
    public bool? Forwarded { get; set; }

    [JsonPropertyName("frequently_forwarded")]
    public bool? FrequentlyForwarded { get; set; }
}

public class TextMessageDto
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class MediaMessageDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("voice")]
    public bool? Voice { get; set; }
}

public class DocumentMessageDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}

public class ButtonMessageDto
{
    [JsonPropertyName("payload")]
    public string? Payload { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class InteractiveMessageDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("button_reply")]
    public ButtonReplyDto? ButtonReply { get; set; }

    [JsonPropertyName("list_reply")]
    public ListReplyDto? ListReply { get; set; }

    [JsonPropertyName("nfm_reply")]
    public NfmReplyDto? NfmReply { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}

public class ButtonReplyDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class ListReplyDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class NfmReplyDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("response_json")]
    public string? ResponseJson { get; set; }
}

public class LocationMessageDto
{
    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

public class SharedContactDto
{
    [JsonPropertyName("name")]
    public SharedContactNameDto? Name { get; set; }

    [JsonPropertyName("phones")]
    public List<SharedContactPhoneDto>? Phones { get; set; }

    [JsonPropertyName("emails")]
    public List<SharedContactEmailDto>? Emails { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}

public class SharedContactNameDto
{
    [JsonPropertyName("formatted_name")]
    public string? FormattedName { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
}

public class SharedContactPhoneDto
{
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("wa_id")]
    public string? WaId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class SharedContactEmailDto
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ReactionMessageDto
{
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("emoji")]
    public string? Emoji { get; set; }
}

public class ReferralDto
{
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    [JsonPropertyName("source_type")]
    public string? SourceType { get; set; }

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("ctwa_clid")]
    public string? CtwaClid { get; set; }
}

public class StatusDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("recipient_id")]
    public string? RecipientId { get; set; }

    [JsonPropertyName("conversation")]
    public ConversationDto? Conversation { get; set; }

    [JsonPropertyName("pricing")]
    public PricingDto? Pricing { get; set; }

    [JsonPropertyName("errors")]
    public List<WebhookErrorDto>? Errors { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}

public class ConversationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("origin")]
    public ConversationOriginDto? Origin { get; set; }

    [JsonPropertyName("expiration_timestamp")]
    public string? ExpirationTimestamp { get; set; }
}

public class ConversationOriginDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class PricingDto
{
    [JsonPropertyName("billable")]
    public bool? Billable { get; set; }

    [JsonPropertyName("pricing_model")]
    public string? PricingModel { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class WebhookErrorDto
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error_data")]
    public ErrorDataDto? ErrorData { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}

public class ErrorDataDto
{
    [JsonPropertyName("details")]
    public string? Details { get; set; }
}