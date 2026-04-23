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
}
