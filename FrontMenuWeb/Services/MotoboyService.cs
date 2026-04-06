using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class MotoboyService
{
    private readonly HttpClient _http;

    public MotoboyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsMotoboy>> GetMotoboysAsync()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsMotoboy>>("motoboys");
        return response?.Data.Lista ?? new List<ClsMotoboy>();
    }

    public async Task<ClsMotoboy> GetMotoboyByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsMotoboy>>($"motoboys/{id}");
        return response?.Data.Objeto ?? new ClsMotoboy();
    }

    public async Task<ReturnApiRefatored<ClsMotoboy>> CreateMotoboyAsync(ClsMotoboy motoboy)
    {
        var response = await _http.PostAsJsonAsync("motoboys", motoboy);
        var returnCorreto = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMotoboy>>();
        return returnCorreto ?? new ReturnApiRefatored<ClsMotoboy>();
    }

    public async Task<ReturnApiRefatored<ClsMotoboy>> UpdateMotoboyAsync(ClsMotoboy motoboy)
    {
        var response = await _http.PatchAsJsonAsync($"motoboys/{motoboy.Id}", motoboy);
        var returnCorreto = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMotoboy>>();
        return returnCorreto ?? new ReturnApiRefatored<ClsMotoboy>();
    }

    public async Task<ReturnApiRefatored<ClsMotoboy>> DeleteMotoboyAsync(ClsMotoboy motoboy)
    {
        var response = await _http.DeleteAsync($"motoboys/{motoboy.Id}");
        var returnCorreto = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMotoboy>>();
        return returnCorreto ?? new ReturnApiRefatored<ClsMotoboy>();
    }
}
