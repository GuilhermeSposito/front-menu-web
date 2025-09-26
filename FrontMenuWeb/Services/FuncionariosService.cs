using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class FuncionariosService
{
    private HttpClient _http;
    public FuncionariosService(HttpClient http)
    {
        _http = http;
    }
    public async Task<ReturnApiRefatored<ClsFuncionario>> GetFuncionariosAutoComplete(string? queryName)
    {
        ReturnApiRefatored<ClsFuncionario>? response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsFuncionario>>($"funcionarios/find/auto-complete?queryName={queryName}");
        return response ?? new ReturnApiRefatored<ClsFuncionario>();
    }

    public async Task<PaginatedResponse<ClsFuncionario>> GetFuncionariosPorPaginaAsync(int page, int pageSize, string? PesquisaDeNome = null)
    {
        string? PesquisaNomeNaUrl = string.Empty;
        if (!string.IsNullOrEmpty(PesquisaDeNome))
            PesquisaNomeNaUrl = $"&nome={PesquisaDeNome}";

        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsFuncionario>>(
           $"funcionarios/pagination?page={page}&limit={pageSize}{PesquisaNomeNaUrl}");

        return response!;
    }

    public async Task<ClsFuncionario> GetFuncionarioByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ClsFuncionario>($"funcionarios/{id}");
        return response ?? new ClsFuncionario();
    }
}
