using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Vendas;
using SocketIO.Core;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace FrontMenuWeb.Services;

public class CaixaEPagamentosService
{
    public HttpClient _HttpClient { get; set; }
    public CaixaEPagamentosService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> VerificaSeHaCaixaAberto(int? FuncionarioId = null)
    {
        string QueryString = FuncionarioId.HasValue ? $"?funcionario_id={FuncionarioId.Value}" : string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Get, $"caixas/aberto{QueryString}");
        var response = await _HttpClient.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<ClsPedido>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return retorno ?? new ReturnApiRefatored<ClsPedido>
        {
            Status = "error",
            Messages = ["Erro ao buscar caixas abertos"]
        };
    }

    public async Task<ReturnApiRefatored<ClsPedido>> AbreCaixa(AbreCaixaDto dto)
    {
        var response = await _HttpClient.PostAsJsonAsync("caixas", dto);
        string json = await response.Content.ReadAsStringAsync();
        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<ClsPedido>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return retorno ?? new ReturnApiRefatored<ClsPedido>
        {
            Status = "error",
            Messages = ["Erro ao abrir caixa"]
        };
    }

    public async Task<ReturnApiRefatored<ClsFechamentoDeCaixa>> FechaCaixa(FechaCaixaDto dto)
    {
        var response = await _HttpClient.PostAsJsonAsync("caixas/fechar", dto);
        string json = await response.Content.ReadAsStringAsync();
        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<ClsFechamentoDeCaixa>>(json);
        return retorno ?? new ReturnApiRefatored<ClsFechamentoDeCaixa>
        {
            Status = "error",
            Messages = ["Erro ao fechar caixa"]
        };
    }



}

