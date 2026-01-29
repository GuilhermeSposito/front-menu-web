using FrontMenuWeb.Models;
using FrontMenuWeb.Models.EntregaMachine;
using FrontMenuWeb.Models.Raios;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class EntregasService
{
    private HttpClient _http;
    public EntregasService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RaioDeEntrega>> GetRaiosDeEntregaAsync()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>("api-entregas/raio");
        return response?.Data.Lista ?? new List<RaioDeEntrega>();
    }

    public async Task<ReturnApiRefatored<RaioDeEntrega>> CreateRaio(RaioDeEntrega raio)
    {
        var response = await _http.PostAsJsonAsync($"api-entregas/raio", raio);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>() ?? new ReturnApiRefatored<RaioDeEntrega>() { Status = "error", Messages = new List<string> { "Erro ao Criar Raio de Entrega"} };
    }

    public async Task<ReturnApiRefatored<RaioDeEntrega>> UpdateRaio(RaioDeEntrega raio)
    {
        var response = await _http.PatchAsJsonAsync($"api-entregas/raio/{raio.Id}", raio);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>() ?? new ReturnApiRefatored<RaioDeEntrega>() { Status = "error", Messages = new List<string> { "Erro ao Atualizar Raio de Entrega"} };
    }

    public async Task<ReturnApiRefatored<RaioDeEntrega>> DeleteRaio(RaioDeEntrega raio)
    {
        var response = await _http.DeleteAsync($"api-entregas/raio/{raio.Id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>() ?? new ReturnApiRefatored<RaioDeEntrega>() { Status = "error", Messages = new List<string> { "Erro ao Atualizar Raio de Entrega"} };
    }
}
