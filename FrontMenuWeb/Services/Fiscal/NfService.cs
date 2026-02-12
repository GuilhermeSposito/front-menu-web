using Blazored.LocalStorage;
using FrontMenuWeb.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Fiscal;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MudBlazor.CategoryTypes;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services.Fiscal;

public class NfService : INfService
{
    private readonly ILocalStorageService _localStorage;
    public HttpClient _http { get; set; }
    public PedidosService _pedidoService { get; set; }
    public NfService(ILocalStorageService localStorage, HttpClient http, PedidosService pedidoService)
    {
        _localStorage = localStorage;
        _http = http;
        _pedidoService = pedidoService;
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

        /*if (!httpResponse.IsSuccessStatusCode)
            return new ReturnApiRefatored<NfeReturnDto>() { Status = "error", Messages = new List<string> { "Erro ao enviar NFC-e"} } ;
        */

        var content = await httpResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReturnApiRefatored<NfeReturnDto>>(content);
        return result ?? new ReturnApiRefatored<NfeReturnDto>();
    }

    public async Task<ReturnApiRefatored<NfeReturnDto>> GeraNFe(ClsPedido Pedido)
    {
        var httpResponse = await _http.PostAsJsonAsync("nf/enviar-nfe", Pedido);

        var resposta = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
            return new ReturnApiRefatored<NfeReturnDto>() { Status = "error", Messages = new List<string> { "Erro ao enviar NF-e", $"Motivo da rejeição {resposta}" } } ;

        var content = await httpResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReturnApiRefatored<NfeReturnDto>>(content);
        return result ?? new ReturnApiRefatored<NfeReturnDto>();
    }


    #endregion

    #region Consulta de NF-e e NFC-e
    public async Task<PaginatedResponse<NFEmitidasDto>> ConsultaNFeEmitidas(DateTime? DataCriadoEmInicio, DateTime? DataCriadoEmFinal, int page = 1, int limit = 10)
    {
        return await _pedidoService.ConsultaNFeEmitidas(DataCriadoEmInicio, DataCriadoEmFinal, page, limit);    
    }
    #endregion

    #region Delete de NF-e e NFC-e
    public async Task<ReturnApiRefatored<NFEmitidasDto>> DeleteRegistroDaNF(int id)
    {
        return await _pedidoService.DeleteRegistroDaNF(id);
    }

    public async Task<ReturnApiRefatored<NFEmitidasDto>> CancelaNFe(string chaveNfe, string Protocolo, int TipoNf ,string Motivo = "Teste de motivo de cancelamento")
    {
        var response = await _http.PostAsJsonAsync($"nf/cancelar-nfce", new CancelaNFDto { ChNfe = chaveNfe, NumeroProtocolo = Protocolo, Justificativa = Motivo });
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReturnApiRefatored<NFEmitidasDto>>(content);
        return result ?? new ReturnApiRefatored<NFEmitidasDto>();
    }
    #endregion
}

public interface INfService
{
    Task<ReturnApiRefatored<EnNfCeDto>> VerificaStatusServicoNFCe();
    Task<ReturnApiRefatored<NfeReturnDto>> GeraNFCe(EnNfCeDto envNFCeDto);
    Task<ReturnApiRefatored<NfeReturnDto>> GeraNFe(ClsPedido Pedido);
    Task<PaginatedResponse<NFEmitidasDto>> ConsultaNFeEmitidas(DateTime? DataCriadoEmInicio, DateTime? DataCriadoEmFinal, int page = 1, int limit = 10);
    Task<ReturnApiRefatored<NFEmitidasDto>> DeleteRegistroDaNF(int id);
}
