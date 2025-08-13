using FrontMenuWeb.Models.Financeiro;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models;

public class ReturnApiRefatored<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public List<string> Messages { get; set; } = new List<string>();

    [JsonPropertyName("data")]
    public Data<T> Data { get; set; } = new Data<T>();
}

public class Data<T>
{
    [JsonPropertyName("Categorias")]
    public List<T> ListaDeObjetosRetornadoCategorias { get; set; } = new List<T>();

    [JsonPropertyName("Categoria")]
    public T? ObjetoRetornadoCategoria { get; set; }

    [JsonPropertyName("SubCategoria")]
    public T? ObjetoRetornadoSubCategoria { get; set; }
}
