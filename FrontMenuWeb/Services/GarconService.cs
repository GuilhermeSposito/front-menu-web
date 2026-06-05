using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class GarconService
{
    private readonly HttpClient _http;

    public GarconService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ReturnApiRefatored<ClsGarcon>> GetGarconsAsync()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsGarcon>>("garcons");
        return response ?? new ReturnApiRefatored<ClsGarcon>();
    }

    public async Task<ClsGarcon> GetGarconByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsGarcon>>($"garcons/{id}");
        return response?.Data.Objeto ?? new ClsGarcon();
    }

    public async Task<ReturnApiRefatored<ClsGarcon>> CreateGarconAsync(ClsGarcon garcon)
    {
        var response = await _http.PostAsJsonAsync("garcons", garcon);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsGarcon>>();
        return result ?? new ReturnApiRefatored<ClsGarcon>();
    }

    public async Task<ReturnApiRefatored<ClsGarcon>> UpdateGarconAsync(ClsGarcon garcon)
    {
        var response = await _http.PutAsJsonAsync($"garcons/{garcon.Id}", garcon);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsGarcon>>();
        return result ?? new ReturnApiRefatored<ClsGarcon>();
    }

    public async Task<ReturnApiRefatored<ClsGarcon>> ToggleAtivoGarconAsync(int id, bool ativo)
    {
        var response = await _http.PutAsJsonAsync($"garcons/{id}", new { ativo });
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsGarcon>>();
        return result ?? new ReturnApiRefatored<ClsGarcon>();
    }

    public async Task<ReturnApiRefatored<ClsGarcon>> DeleteGarconAsync(int id)
    {
        var response = await _http.DeleteAsync($"garcons/{id}");
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsGarcon>>();
        return result ?? new ReturnApiRefatored<ClsGarcon>();
    }

    public async Task<ReturnApiRefatored<List<ClsMesasEComandas>>> GetMesasAsync()
    {
        var response = await _http.GetAsync("garcons/mesas");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<List<ClsMesasEComandas>>>();
    }

    public async Task<DtoItensPorGarcom?> GetItensPorGarcomAsync(
        int? garconId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        string? nomeItem = null,
        int page = 1,
        int limit = 10)
    {
        var parametros = new List<string> { $"page={page}", $"limit={limit}" };
        if (garconId.HasValue) parametros.Add($"garconId={garconId}");
        if (dataInicio.HasValue) parametros.Add($"dataInicio={dataInicio:yyyy-MM-dd}");
        if (dataFim.HasValue) parametros.Add($"dataFim={dataFim:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(nomeItem)) parametros.Add($"nomeItem={Uri.EscapeDataString(nomeItem)}");

        var url = "garcons/itens-lancados?" + string.Join("&", parametros);
        var response = await _http.GetAsync(url);
        var retorno = await response.Content.ReadFromJsonAsync<RespostaItensPorGarcom>();
        return retorno?.Data;
    }

    private class RespostaItensPorGarcom
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("data")] public DtoItensPorGarcom? Data { get; set; }
    }
}
