using FrontMenuWeb.Models.Financeiro;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services.FinanceroServices;

public class ContasService
{
    public HttpClient _HttpClient { get; set; }
    public ContasService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsConta>?> GetContasAsync()
    {
        var response = await _HttpClient.GetAsync("financeiro/contas");

        RetornoApiContas retornoApi = await response.Content.ReadFromJsonAsync<RetornoApiContas>() ?? new RetornoApiContas();

        return retornoApi.Data.Contas;
    }
    public async Task<ClsConta?> GetContaAsync(int IdDaConta)
    {
        var response = await _HttpClient.GetAsync($"financeiro/contas/{IdDaConta}");
        var ReturnApi = await response.Content.ReadFromJsonAsync<RetornoApiContas>();

        return ReturnApi!.Data.Conta;
    }

    public async Task<RetornoApiContas> AdicionaContaAsync(ClsConta conta)
    {
        var response = await _HttpClient.PostAsJsonAsync("financeiro/contas", conta);
        return await response.Content.ReadFromJsonAsync<RetornoApiContas>() ?? new RetornoApiContas();
    }

    public async Task<RetornoApiContas> AtualizaContaAsync(ClsConta conta)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"financeiro/contas/{conta.Id}", conta);
        return await response.Content.ReadFromJsonAsync<RetornoApiContas>() ?? new RetornoApiContas();
    }

    public async Task<RetornoApiContas> DeletaContaAsync(int IdDaConta)
    {
        var response = await _HttpClient.DeleteAsync($"financeiro/contas/{IdDaConta}");
        return await response.Content.ReadFromJsonAsync<RetornoApiContas>() ?? new RetornoApiContas();
    }

}
