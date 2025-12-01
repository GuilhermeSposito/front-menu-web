using Blazored.LocalStorage;
using FrontMenuWeb.Dtos;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.Security;
using static MudBlazor.CategoryTypes;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services.Fiscal;

public class NfService
{
    private readonly ILocalStorageService _localStorage;
    public HttpClient _http { get; set; }
    public NfService(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public async Task<ReturnApiRefatored<EnNfCeDto>> VerificaStatusServicoNFCe()
    {
        //var token = await _localStorage.GetItemAsStringAsync("token");

       // _http.DefaultRequestHeaders.Authorization =
            //new AuthenticationHeaderValue("Bearer", token);

        var httpResponse = await _http.GetAsync("nf/status-nfce");

        Console.WriteLine(httpResponse.StatusCode);

        if (!httpResponse.IsSuccessStatusCode)
            return new ReturnApiRefatored<EnNfCeDto>() { Status = "error", Messages = new List<string> { "Erro ao consultar status NFC-e"} } ;

        var content = await httpResponse.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ReturnApiRefatored<EnNfCeDto>>(content);

        return result ?? new ReturnApiRefatored<EnNfCeDto>();
    }
}
