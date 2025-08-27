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

    public async Task<PaginatedResponse<ClsLancamentoFinanceiro>> GetLancamentosPorPagina(int page, int pageSize)
    {
        var response = await _HttpClient.GetFromJsonAsync<PaginatedResponse<ClsLancamentoFinanceiro>>(
           $"financeiro/lancamentos?page={page}&limit={pageSize}");

        return response!;
    }

    public async Task<ClsLancamentoFinanceiro> GetLancamentoAsync(int idDoLancamento)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsLancamentoFinanceiro>>(
           $"financeiro/lancamentos/{idDoLancamento}");

        return response!.Data.Objeto ?? new ClsLancamentoFinanceiro();
    }

}
