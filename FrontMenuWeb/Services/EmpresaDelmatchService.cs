using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class EmpresaDelmatchService
{
    private readonly HttpClient _http;

    public EmpresaDelmatchService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsEmpresaDelmatch>> GetEmpresasIntegradas()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsEmpresaDelmatch>>("empresas-delmatch");
        return response?.Data.Lista ?? new List<ClsEmpresaDelmatch>();
    }

    public async Task<ReturnApiRefatored<ClsEmpresaDelmatch>> CreateEmpresa(ClsEmpresaDelmatch empresa)
    {
        var response = await _http.PostAsJsonAsync("empresas-delmatch", empresa);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaDelmatch>>();
        return result ?? new ReturnApiRefatored<ClsEmpresaDelmatch>();
    }

    public async Task<ReturnApiRefatored<ClsEmpresaDelmatch>> UpdateEmpresa(ClsEmpresaDelmatch empresa)
    {
        var response = await _http.PatchAsJsonAsync($"empresas-delmatch/{empresa.Id}", empresa);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaDelmatch>>();
        return result ?? new ReturnApiRefatored<ClsEmpresaDelmatch>();
    }

    public async Task<ReturnApiRefatored<ClsEmpresaDelmatch>> DeleteEmpresa(int id)
    {
        var response = await _http.DeleteAsync($"empresas-delmatch/{id}");
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaDelmatch>>();
        return result ?? new ReturnApiRefatored<ClsEmpresaDelmatch>();
    }
}
