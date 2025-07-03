using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class AliquotaService
{
    public HttpClient _HttpClient { get; set; }
    public AliquotaService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsAliquota>> GetAliquotas()
    {
        var response = await _HttpClient.GetFromJsonAsync<List<ClsAliquota>>("aliquotas");
        return response ?? new List<ClsAliquota>();
    }
    public async Task<ClsAliquota> GetAliquota(ClsAliquota Aliquota)
    {
        var response = await _HttpClient.GetFromJsonAsync<ClsAliquota>($"aliquotas/{Aliquota.Id}");
        return response ?? new ClsAliquota();
    }

    public async Task<HttpResponseMessage> AdicionaAliquota(ClsAliquota NovaAlicota)
    {
        var response = await _HttpClient.PostAsJsonAsync("aliquotas/create", NovaAlicota);
        return response;
    }

    public async Task<HttpResponseMessage> EditaAliquota(ClsAliquota Aliquota)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"aliquotas/update/{Aliquota.Id}", Aliquota);
        return response;
    }

    public async Task<HttpResponseMessage> DeletarALiquota(ClsAliquota Aliquota)
    {
        var response = await _HttpClient.DeleteAsync($"aliquotas/{Aliquota.Id}");
        return response;
    }
}
