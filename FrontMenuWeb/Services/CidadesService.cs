using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pessoas;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class CidadesService
{
    private readonly HttpClient _http;

    public CidadesService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsCidade>> GetCidades()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsCidade>>("cidades");
        return response?.Data.Lista ?? new List<ClsCidade>();
    }
}
