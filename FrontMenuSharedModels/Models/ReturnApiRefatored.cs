using FrontMenuWeb.Models.Financeiro;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models;

public class ReturnApiRefatored<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public List<string> Messages { get; set; } = new List<string>();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("data")] public Data<T> Data { get; set; } = new Data<T>();
}

public class Data<T>
{
    [JsonExtensionData] public Dictionary<string, JsonElement> ExtraData { get; set; } = new();

    [JsonPropertyName("message")] public List<string> Messages { get; set; } = new List<string>();

    private static readonly JsonSerializerOptions _defaultOptions = new() { PropertyNameCaseInsensitive = true };

    [JsonIgnore]
    public List<T>? Lista
    {
        get
        {
            foreach (var item in ExtraData)
            {
                if (item.Value.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(item.Value.GetRawText(), _defaultOptions);
                }
            }
            return null;
        }
    }

    [JsonIgnore]
    public T? Objeto
    {
        get
        {
            foreach (var item in ExtraData)
            {
                if (item.Value.ValueKind == JsonValueKind.Object)
                {
                    return JsonSerializer.Deserialize<T>(item.Value.GetRawText(), _defaultOptions);
                }
            }
            return default;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("objetoWhenWriting")]public T? ObjetoWhenWriting { get; set; }
    [JsonPropertyName("ListWhenWriting")]public List<T>? ListWhenWriting { get; set; }
}
