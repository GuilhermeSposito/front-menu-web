using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using YamlDotNet.Core.Tokens;

namespace FrontMenuWeb.Services;

public class PedidosService
{
    private HttpClient _http;
    public PedidosService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResponse<ClsPedido>> GetPedidosPorPaginaAsync(QuerysDePedidos QueryDePedido)
    {
        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsPedido>>(
           $"pedidos?limit={QueryDePedido.PageSize}&page={QueryDePedido.Page}");

        return response!;
    }
}


public class QuerysDePedidos
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 300;

    public TiposDePedido? TipoPedido = null;

    public StatusPedidos? Status = null;

    public DateTime? DataCriadoEmInicio = null;


    public DateTime? DataCriadoEmFinal = null;

    public string CriadoPor = string.Empty;
}