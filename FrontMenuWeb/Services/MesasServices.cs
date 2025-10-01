using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class MesasServices
{
    private HttpClient _http;
    public MesasServices(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResponse<ClsMesasEComandas>> GetMesaPorPaginaAsync(int page, int pageSize)
    {
        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsMesasEComandas>>(
           $"mesas-comandas/pagination?page={page}&limit={pageSize}");

        return response!;
    }

    public async Task<ClsMesasEComandas> GetMesaAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsMesasEComandas>>($"mesas-comandas/{id}");
        return response?.Data.Objeto ?? new ClsMesasEComandas();
    }

    public async Task<ReturnApiRefatored<ClsMesasEComandas>> CreateMesaAsync(ClsMesasEComandas Mesa)
    {
        var response = await _http.PostAsJsonAsync("mesas-comandas", Mesa);
        var createdMesa = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMesasEComandas>>();
        return createdMesa ?? new ReturnApiRefatored<ClsMesasEComandas>();
    }

    public async Task<ReturnApiRefatored<ClsMesasEComandas>> UpdateMesaAsync(ClsMesasEComandas Mesa)
    {
        var response = await _http.PatchAsJsonAsync($"mesas-comandas/{Mesa.Id}", Mesa);
        var updatedMesa = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMesasEComandas>>();
        return updatedMesa ?? new ReturnApiRefatored<ClsMesasEComandas>();
    }

    public async Task<ReturnApiRefatored<ClsMesasEComandas>> DeleteMesaAsync(List<int> idsDasMesas)
    {
        var response = await _http.DeleteAsync($"mesas-comandas?ids={string.Join(",", idsDasMesas)}");
        var deletedMesa = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMesasEComandas>>();
        return deletedMesa ?? new ReturnApiRefatored<ClsMesasEComandas>();
    }
}
