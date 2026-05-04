using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Vendas;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class ConveniosService
{
    private readonly HttpClient _http;

    public ConveniosService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ConveniosPagedResponse> GetConveniosAsync(int page, int limit, string? status = null, int? pessoaId = null)
    {
        var query = $"convenios?page={page}&limit={limit}";
        if (!string.IsNullOrEmpty(status)) query += $"&Status={status}";
        if (pessoaId.HasValue) query += $"&PessoaId={pessoaId}";

        var response = await _http.GetFromJsonAsync<ConveniosPagedResponse>(query);
        return response ?? new ConveniosPagedResponse();
    }

    public async Task<ClsConvenio?> GetConvenioPorClienteAsync(int pessoaId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsConvenio>>($"convenios/cliente/{pessoaId}");
            return response?.Data.Objeto;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ClsConvenioPedido>> GetPedidosDoConvenioAsync(int convenioId)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsConvenioPedido>>($"convenios/{convenioId}/pedidos");
        return response?.Data.Lista ?? new List<ClsConvenioPedido>();
    }

    public async Task<List<ClsConvenioPagamento>> GetPagamentosDoConvenioAsync(int convenioId)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsConvenioPagamento>>($"convenios/{convenioId}/pagamentos");
        return response?.Data.Lista ?? new List<ClsConvenioPagamento>();
    }

    public async Task<ReturnApiRefatored<ClsPessoas>> RegistrarPagamentoAsync(CriarPagamentoConvenioDto dto)
    {
        var response = await _http.PostAsJsonAsync("convenios/pagamento", dto);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPessoas>>() ?? new ReturnApiRefatored<ClsPessoas>();
    }

    public async Task<ReturnApiRefatored<ClsConvenio>> DeleteConvenioAsync(int id)
    {
        var response = await _http.DeleteAsync($"convenios/{id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsConvenio>>() ?? new ReturnApiRefatored<ClsConvenio>();
    }

    public async Task<ReturnApiRefatored<ClsConvenio>> DeleteConvenioPedidoAsync(int convenioId, int convenioPedidoId)
    {
        var response = await _http.DeleteAsync($"convenios/{convenioId}/pedido/{convenioPedidoId}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsConvenio>>() ?? new ReturnApiRefatored<ClsConvenio>();
    }
}
