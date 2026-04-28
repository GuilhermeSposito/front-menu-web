using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class SendMessageDtoWS
{
    [JsonPropertyName("messaging_product")] public string MessageProduct { get; set; } = "whatsapp";
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("recipient_type")] public string? RecipientType { get { if (Type == TipoMensagem.text) return "individual"; else return null; } }
    [JsonPropertyName("type")] public TipoMensagem Type { get; set; } = TipoMensagem.template;
    [JsonPropertyName("template")] public TemplateDto? Template { get; set; }
    [JsonPropertyName("text")] public TextSimpleMessageDto? Text { get; set; }
}


#region Enum Para o tipo de mensagem (baseado na documentação oficial do WhatsApp Business API)
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoMensagem
{
    template,
    text
}
#endregion

#region Região de DTOs para mensagens do tipo template 
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemplatesName
{
    status_pedido
}

public class TemplateDto
{
    [JsonPropertyName("name")] public TemplatesName Name { get; set; }
    [JsonPropertyName("language")] public LanguageDto Language { get; set; } = new LanguageDto();
    [JsonPropertyName("components")] public List<ComponentDto> Components { get; set; } = new List<ComponentDto>();
}

public class LanguageDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = "pt_BR";
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentType
{
    header,
    body,
    button
}
public class ComponentDto
{
    [JsonPropertyName("type")] public ComponentType Type { get; set; } = ComponentType.body;
    [JsonPropertyName("parameters")] public List<ParameterDto> Parameters { get; set; } = new List<ParameterDto>();

    [JsonPropertyName("sub_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SubType { get; set; }

    [JsonPropertyName("index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Index { get; set; }

}

public class ParameterDto
{
    [JsonPropertyName("type")] public string Type { get; set; } = "text";
    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
}
#endregion

#region DTO para mensagens do tipo texto simples
public class TextSimpleMessageDto
{
    [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;
}
#endregion
