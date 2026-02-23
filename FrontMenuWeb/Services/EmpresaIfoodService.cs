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
    public async Task<string> GerarAutorizacao()
    {
        Console.WriteLine("TESTEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        var HttpIfood = _factory.CreateClient("ApiIfood");

        FormUrlEncodedContent formData = new FormUrlEncodedContent(new[]
        {
              new KeyValuePair<string, string>("clientId", "7e476dce-79fa-4a7e-a605-aa2a1a40b803")
        });
        var response = await HttpIfood.PostAsync("/authentication/v1.0/oauth/userCode", formData);   
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<UserCodeReturnFromAPIIfood>();


        if(result is null)
            return "";

        string? UrlDeVerificacao = result?.VerificationUrlComplete;

        return UrlDeVerificacao ?? "";
    }

}

internal class UserCodeReturnFromAPIIfood
{

    [JsonPropertyName("userCode")]public string? UserCode { get; set; }
    [JsonPropertyName("authorizationCodeVerifier")] public string? AuthorizationCodeVerifier { get; set; }
    [JsonPropertyName("verificationUrl")] public string? VerificationUrl { get; set; }
    [JsonPropertyName("verificationUrlComplete")] public string? VerificationUrlComplete { get; set; }
    [JsonPropertyName("expiresIn")] public int ExpiresIn { get; set; }


}