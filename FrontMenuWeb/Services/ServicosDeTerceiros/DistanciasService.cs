using FrontMenuWeb.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services.ServicosDeTerceiros;

public class DistanciasService
{
    private readonly HttpClient _http;

    public DistanciasService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ReturnApiRefatored<ClsRetornoDeSomaDaDistancia>> ConsultarDistanciaAsync(string origem, string destino)
    {
        var queryDestinationAndOrigins = $"api-entregas?origem={origem}&destino={destino}";
        var response = await _http.GetAsync(queryDestinationAndOrigins);

        var ResponseApi = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsRetornoDeSomaDaDistancia>>();

        return ResponseApi;
    }
}

public class ClsRetornoDeSomaDaDistancia
{
    [JsonPropertyName("distanciaKm")] public float DistanciaTotalEmKm { get; set; }
    [JsonPropertyName("valorTotal")] public float ValorTotal { get; set; }
}