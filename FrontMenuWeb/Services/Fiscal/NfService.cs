using Blazored.LocalStorage;
using FrontMenuWeb.Dtos;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Fiscal;
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

    #region Verificações de Serviço

    public async Task<ReturnApiRefatored<EnNfCeDto>> VerificaStatusServicoNFCe()
    {
        var httpResponse = await _http.GetAsync("nf/status-nfce");

        if (!httpResponse.IsSuccessStatusCode)
            return new ReturnApiRefatored<EnNfCeDto>() { Status = "error", Messages = new List<string> { "Erro ao consultar status NFC-e"} } ;

        var content = await httpResponse.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ReturnApiRefatored<EnNfCeDto>>(content);

        return result ?? new ReturnApiRefatored<EnNfCeDto>();
    }
    #endregion

    #region Emissões de NF-e e NFC-e
    public async Task<ReturnApiRefatored<NfeReturnDto>> GeraNFCe(EnNfCeDto envNFCeDto)
    {
        var httpResponse = await _http.PostAsJsonAsync("nf/enviar-nfce", envNFCeDto);
        if (!httpResponse.IsSuccessStatusCode)
            return new ReturnApiRefatored<NfeReturnDto>() { Status = "error", Messages = new List<string> { "Erro ao enviar NFC-e"} } ;

        var content = await httpResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReturnApiRefatored<NfeReturnDto>>(content);
        return result ?? new ReturnApiRefatored<NfeReturnDto>();
    }
    #endregion
}
