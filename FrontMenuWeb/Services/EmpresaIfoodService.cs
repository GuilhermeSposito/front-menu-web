using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services;

public class EmpresaIfoodService
{
    private HttpClient _http;
    private readonly IHttpClientFactory _factory;
    public EmpresaIfoodService(HttpClient http, IHttpClientFactory factory)
    {
        _http = http;
        _factory = factory;
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


    //Funções de Integrações com Api Ifood
    public async Task<string?> GerarAutorizacao()
    {
        var HttpIntegracoes = _factory.CreateClient("ApiIntegracoes");
        var response = await HttpIntegracoes.GetAsync("ifood/authorization");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<UserCodeReturnFromAPIIfoodDto>>();
            return result?.Data.Objeto?.VerificationUrlComplete ?? string.Empty;
        }
        else
        {
            throw new Exception("Erro ao obter autorização do iFood.");
        }
    }

}

