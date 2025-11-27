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

    [JsonPropertyName("data")] public Data<T> Data { get; set; } = new Data<T>();
}

public class Data<T>
{
    [JsonExtensionData] public Dictionary<string, JsonElement> ExtraData { get; set; } = new();

    [JsonPropertyName("message")] public List<string> Messages { get; set; } = new List<string>();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<T>? Lista
    {
        get
        {
            foreach (var item in ExtraData)
            {
                if (item.Value.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(item.Value.GetRawText());
                }
            }
            return null;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Objeto
    {
        get
        {
            foreach (var item in ExtraData)
            {
                if (item.Value.ValueKind == JsonValueKind.Object)
                {
                    return JsonSerializer.Deserialize<T>(item.Value.GetRawText());
                }
            }
            return default;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? ObjetoWhenWriting { get; set; }
}



































/*[JsonPropertyName("Categorias")]
public List<T> ListaDeObjetosRetornadoCategorias { get; set; } = new List<T>();

[JsonPropertyName("Categoria")]
public T? ObjetoRetornadoCategoria { get; set; }

[JsonPropertyName("SubCategoria")]
public T? ObjetoRetornadoSubCategoria { get; set; }*/


/*public List<T>? GetList(string key)
{
    if (ExtraData.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.Array)
    {
        return JsonSerializer.Deserialize<List<T>>(element.GetRawText());
    }
    return null;
}

public T? GetSingle(string key)
{
    if (ExtraData.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.Object)
    {
        return JsonSerializer.Deserialize<T>(element.GetRawText());
    }
    return default;
}*/