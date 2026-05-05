using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class HorariosMerchantService
{
    private readonly HttpClient _http;

    public HorariosMerchantService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsHorarioMerchant>> GetHorariosAsync()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsHorarioMerchant>>("horarios-merchant");
        return response?.Data.Lista ?? new List<ClsHorarioMerchant>();
    }

    public async Task<ReturnApiRefatored<ClsHorarioMerchant>> CriarHorarioAsync(CriarHorarioMerchantDto dto)
    {
        var response = await _http.PostAsJsonAsync("horarios-merchant", dto);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsHorarioMerchant>>() ?? new ReturnApiRefatored<ClsHorarioMerchant>();
    }

    public async Task<ReturnApiRefatored<ClsHorarioMerchant>> AtualizarHorarioAsync(int id, AtualizarHorarioMerchantDto dto)
    {
        var response = await _http.PatchAsJsonAsync($"horarios-merchant/{id}", dto);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsHorarioMerchant>>() ?? new ReturnApiRefatored<ClsHorarioMerchant>();
    }

    public async Task<bool> DeletarHorarioAsync(int id)
    {
        var response = await _http.DeleteAsync($"horarios-merchant/{id}");
        return response.IsSuccessStatusCode;
    }
}
