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

    // ─── Motoboys ────────────────────────────────────────────────────────────

    public async Task<List<ClsMotoboy>> GetMotoboysAsync(bool? apenasAtivos = null)
    {
        var url = apenasAtivos.HasValue ? $"motoboys?ativo={apenasAtivos.Value.ToString().ToLower()}" : "motoboys";
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsMotoboy>>(url);
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

    // ─── Entregas ─────────────────────────────────────────────────────────────

    public async Task<List<ClsPedidoMotoboy>> GetTodasAsEntregasAsync()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsPedidoMotoboy>>("motoboys/all/entregas");
        return response?.Data.Lista ?? new List<ClsPedidoMotoboy>();
    }

    public async Task<ReturnApiRefatored<ClsPedidoMotoboy>> CriarEntregaAsync(ClsPedidoMotoboy entrega)
    {
        var response = await _http.PostAsJsonAsync("motoboys/entregas", entrega);
        var returnCorreto = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedidoMotoboy>>();
        return returnCorreto ?? new ReturnApiRefatored<ClsPedidoMotoboy>();
    }

    public async Task<ReturnApiRefatored<ClsPedidoMotoboy>> DeletarEntregaAsync(int id)
    {
        var response = await _http.DeleteAsync($"motoboys/entregas/{id}");
        var returnCorreto = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedidoMotoboy>>();
        return returnCorreto ?? new ReturnApiRefatored<ClsPedidoMotoboy>();
    }

    // ─── Distância ───────────────────────────────────────────────────────────

    public async Task<ClsDistanciaEntrega?> GetDistanciaEntregaAsync(string origem, string destino)
    {
        var url = $"api-entregas?origem={Uri.EscapeDataString(origem)}&destino={Uri.EscapeDataString(destino)}";
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsDistanciaEntrega>>(url);
        return response?.Data.Objeto;
    }
}
