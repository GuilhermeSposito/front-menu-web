using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using SixLabors.ImageSharp;
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

    public async Task<ClsEmpresaIfood> GetEmpresaIntegradaAsync(int idEmpresa)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>($"empresas-ifood/{idEmpresa}");
        return response?.Data.Objeto ?? new ClsEmpresaIfood();
    }

    public async Task<ClsEmpresaIfood?> GetEmpresaIntegradaPeloMerchantIdAsync(string idEmpresa)
    {
        var response = await _http.GetAsync($"empresas-ifood/{idEmpresa}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>();

        return result?.Data.Objeto;
    }

    public async Task<ReturnApiRefatored<ClsEmpresaIfood>> CreateEmpresa(ClsEmpresaIfood empresa)
    {
        var response = await _http.PostAsJsonAsync<ClsEmpresaIfood>("empresas-ifood", empresa);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsEmpresaIfood>>();

        return result ?? new ReturnApiRefatored<ClsEmpresaIfood>();
    }
    public async Task<ReturnApiRefatored<ClsEmpresaIfood>> CreateEmpresaPublic(ClsEmpresaIfood empresa, string MerchantSophosID)
    {
        var response = await _http.PostAsJsonAsync<ClsEmpresaIfood>($"empresas-ifood/public/{MerchantSophosID}", empresa);
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

