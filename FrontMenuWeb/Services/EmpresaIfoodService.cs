using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class EmpresaIfoodService
{
    private HttpClient _http;
    public EmpresaIfoodService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsEmpresaIfood>> GetEmpresasIntegradas()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>("empresas-ifood");
        return response?.Data.Lista ?? new List<ClsEmpresaIfood>();
    }

    public async Task<ReturnApiRefatored<ClsEmpresaIfood>> CreateEmpresa(ClsEmpresaIfood empresa)
    {
        var response = await _http.PostAsJsonAsync<ClsEmpresaIfood>("empresas-ifood", empresa);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>();

        return result ?? new ReturnApiRefatored<ClsEmpresaIfood>();
    }

    public async Task<ReturnApiRefatored<ClsEmpresaIfood>> UpdateEmpresa(ClsEmpresaIfood empresa)
    {
        var response = await _http.PatchAsJsonAsync<ClsEmpresaIfood>($"empresas-ifood/{empresa.Id}", empresa);

        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>();
        return result ?? new ReturnApiRefatored<ClsEmpresaIfood>();
    }

    public async Task<ReturnApiRefatored<ClsEmpresaIfood>> DeleteEmpresa(int id)
    {
        var response = await _http.DeleteAsync($"empresas-ifood/{id}");
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>();

        return result ?? new ReturnApiRefatored<ClsEmpresaIfood>();
    }

}
