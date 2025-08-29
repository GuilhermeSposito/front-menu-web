using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services.FinanceroServices;

//ClsLancamentoFinanceiro
public class LancamentoFinanceiroService
{
    public HttpClient _HttpClient { get; set; }
    public LancamentoFinanceiroService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<PaginatedResponse<ClsLancamentoFinanceiro>> GetLancamentosPorPagina(int page, int pageSize, DateTime? DataInicial = null, DateTime? DataFinal = null, DateTime? DataEmissao = null, bool? Pagos = null, string? FiltroDescricao = null)
    {
        string QuerysStrings = "";

        if(DataInicial is not null && DataFinal is not null)
        {
            QuerysStrings += $"&DataEmissaoInicio={DataInicial?.ToString("yyyy-MM-ddTHH:mm:ssZ")}&DataEmissaoFinal={DataFinal?.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
        }

        if(DataEmissao is not null)
        {
            QuerysStrings += $"&DataEmissao={DataEmissao?.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
        }

        if(Pagos is not null)
        {
            QuerysStrings += $"&Pago={Pagos}";
        }

        if(!string.IsNullOrEmpty(FiltroDescricao))
        {
            QuerysStrings += $"&descricao={FiltroDescricao}";
        }

        var response = await _HttpClient.GetFromJsonAsync<PaginatedResponse<ClsLancamentoFinanceiro>>(
           $"financeiro/lancamentos?page={page}&limit={pageSize}{QuerysStrings}");

        return response!;
    }

    public async Task<ClsLancamentoFinanceiro> GetLancamentoAsync(int idDoLancamento)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsLancamentoFinanceiro>>(
           $"financeiro/lancamentos/{idDoLancamento}");

        return response!.Data.Objeto ?? new ClsLancamentoFinanceiro();
    }

    public async Task<ReturnApiRefatored<ClsLancamentoFinanceiro>> CreateLancamentoAsync(ClsLancamentoFinanceiro novoLancamento)
    {
        var response = await _HttpClient.PostAsJsonAsync("financeiro/lancamentos", novoLancamento);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsLancamentoFinanceiro>>() ?? new ReturnApiRefatored<ClsLancamentoFinanceiro>();
    }
    public async Task<ReturnApiRefatored<ClsLancamentoFinanceiro>> UpdateLancamentoAsync(ClsLancamentoFinanceiro LancamentoAMudar)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"financeiro/lancamentos/{LancamentoAMudar.Id}", LancamentoAMudar);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsLancamentoFinanceiro>>() ?? new ReturnApiRefatored<ClsLancamentoFinanceiro>();
    }

    public async Task<ReturnApiRefatored<ClsLancamentoFinanceiro>> DeleteLancamentoAsync(ClsLancamentoFinanceiro LancamentoAMudar)
    {
        var response = await _HttpClient.DeleteAsync($"financeiro/lancamentos/{LancamentoAMudar.Id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsLancamentoFinanceiro>>() ?? new ReturnApiRefatored<ClsLancamentoFinanceiro>();
    }


}
