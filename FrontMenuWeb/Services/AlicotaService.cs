using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class AlicotaService
{
    public HttpClient _HttpClient { get; set; }
    public AlicotaService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsAlicota>> GetAlicotas()
    {
        var response = await _HttpClient.GetFromJsonAsync<List<ClsAlicota>>("alicotas");
        return response ?? new List<ClsAlicota>();
    }

    public async Task<HttpResponseMessage> AdicionaAlicota(ClsAlicota NovaAlicota)
    {
        var response = await _HttpClient.PostAsJsonAsync("alicotas/create", NovaAlicota);
        return response;
    }

    public async Task<HttpResponseMessage> EditaGrupo(ClsAlicota Alicota)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"alicotas/{Alicota.Id}", Alicota);
        return response;
    }

    public async Task<HttpResponseMessage> DeletarGrupo(ClsAlicota Alicota)
    {
        var response = await _HttpClient.DeleteAsync($"alicotas/{Alicota.Id}");
        return response;
    }
}
