using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class GrupoServices
{
    public HttpClient _HttpClient { get; set; }
    public GrupoServices(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsGrupo>> GetGrupos()
    {
        var response = await _HttpClient.GetFromJsonAsync<List<ClsGrupo>>("grupos");
        return response ?? new List<ClsGrupo>();
    }

    public async Task<List<ClsGrupo>> GetGruposPublicAsync(string IdMerchant)
    {
        var response = await _HttpClient.GetFromJsonAsync<List<ClsGrupo>>($"grupos/public/{IdMerchant}");
        return response ?? new List<ClsGrupo>();
    }

    public async Task<HttpResponseMessage> AdicionaGrupo(ClsGrupo NovoGrupo)
    {
        var response = await _HttpClient.PostAsJsonAsync("grupos/create", NovoGrupo);
        return response;
    }

    public async Task<HttpResponseMessage> EditaGrupo(ClsGrupo Grupo)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"grupos/{Grupo.Id}", Grupo);
        return response;
    } 
    
    public async Task<HttpResponseMessage> DeletarGrupo(ClsGrupo Grupo)
    {
        var response = await _HttpClient.DeleteAsync($"grupos/{Grupo.Id}");
        return response;
    }
}
