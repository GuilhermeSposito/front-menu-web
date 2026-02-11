using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using FrontMenuWeb.Models.Vendas;
using SocketIO.Core;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unimake.Business.DFe.Xml.CTe;
using static System.Net.WebRequestMethods;
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

    public async Task<ReturnApiRefatored<ClsFechamentoDeCaixa>> GetStatusOperacional(FechaCaixaDto? dto = null)
    {
        var response = await _HttpClient.PostAsJsonAsync("caixas/status", dto);
        string json = await response.Content.ReadAsStringAsync();

        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<ClsFechamentoDeCaixa>>(json);
        return retorno ?? new ReturnApiRefatored<ClsFechamentoDeCaixa>
        {
            Status = "error",
            Messages = ["Erro ao fechar caixa"]
        };
    }

    public async Task<ReturnApiRefatored<PagamentoDoPedido>> GetPagamentosDoPedidoAsync(ClsPedido Pedido)
    {
        var response = await _HttpClient.GetAsync($"caixas/pagamentos/{Pedido.Id}");
        string json = await response.Content.ReadAsStringAsync();

        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<PagamentoDoPedido>>(json);
        return retorno ?? new ReturnApiRefatored<PagamentoDoPedido>
        {
            Status = "error",
            Messages = ["Erro ao buscar pagamentos para o pedido."]
        };
    }

    public async Task<ReturnApiRefatored<PagamentoDoPedido>> DeletePagamento(PagamentoDoPedido pagamentoDoPedido)
    {
        var response = await _HttpClient.DeleteAsync($"caixas/pagamentos/{pagamentoDoPedido.Id}");
        string json = await response.Content.ReadAsStringAsync();

        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<PagamentoDoPedido>>(json);
        return retorno ?? new ReturnApiRefatored<PagamentoDoPedido>
        {
            Status = "error",
            Messages = ["Erro ao deletar pagamento para o pedido."]
        };
    }

    public async Task<ReturnApiRefatored<PagamentoDoPedido>> AdicionaPagamentoAoPedido(PagamentoDoPedido pagamentoDoPedido, ClsPedido pedidoCaixa)
    {
        var response = await _HttpClient.PostAsJsonAsync($"caixas/pagamentos/create/{pedidoCaixa.Id}", pagamentoDoPedido);
        string json = await response.Content.ReadAsStringAsync();
        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<PagamentoDoPedido>>(json);
        return retorno ?? new ReturnApiRefatored<PagamentoDoPedido>
        {
            Status = "error",
            Messages = ["Erro ao adicionar pagamento para o pedido."]
        };
    }

    public async Task<PaginatedResponse<Caixa>> GetCaixasFechadosAsync(QueryCaixasDto queryDto)
    {
        var queryParams = new List<string>();

        queryParams.Add($"limit={queryDto.Limit}");
        queryParams.Add($"page={queryDto.page}");

        if (queryDto.DataFechadoEmInicio.HasValue)
            queryParams.Add($"DataFechadoEmInicio={queryDto.DataFechadoEmInicio?.ToString("yyyy-MM-ddTHH:mm:ssZ")}");

        if (queryDto.DataFechadoEmFinal.HasValue)
            queryParams.Add($"DataFechadoEmFinal={queryDto.DataFechadoEmFinal?.ToString("yyyy-MM-ddTHH:mm:ssZ")}");

        var url = "caixas/fechados";

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var response = await _HttpClient.GetFromJsonAsync<PaginatedResponse<Caixa>>($"{url}");

        return response ?? new PaginatedResponse<Caixa>();
    }

}



