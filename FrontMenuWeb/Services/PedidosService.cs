using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core.Tokens;

namespace FrontMenuWeb.Services;

public class PedidosService
{
    private HttpClient _http;
    public PedidosService(HttpClient http)
    {
        _http = http;
    }

    public static event Action<ClsPedido>? PedidoRecebido;
    public static event Action<PedidoMesaDto>? PedidoMesaRecebido;


    [JSInvokable]
    public static Task ReceivePedido(string msg)
    {
        ClsPedido pedido = System.Text.Json.JsonSerializer.Deserialize<ClsPedido>(msg)!;

        PedidoRecebido?.Invoke(pedido);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public static Task ReceivePedidoMesa(string msg)
    {
        PedidoMesaDto pedido = System.Text.Json.JsonSerializer.Deserialize<PedidoMesaDto>(msg)!;

        PedidoMesaRecebido?.Invoke(pedido);
        return Task.CompletedTask;
    }

    public async Task<PaginatedResponse<ClsPedido>> GetPedidosPorPaginaAsync(QuerysDePedidos QueryDePedido)
    {
        string QueryDeFiltros = "";

        if (QueryDePedido.Status != null)
        {
            QueryDeFiltros += $"&Status={QueryDePedido.Status}";
        }

        if (!String.IsNullOrEmpty(QueryDePedido.Pesquisa))
        {
            QueryDeFiltros += $"&pesquisa={QueryDePedido.Pesquisa}";
        }

        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsPedido>>(
           $"pedidos?limit={QueryDePedido.PageSize}&page={QueryDePedido.Page}{QueryDeFiltros}");

        return response!;
    }

    public async Task<PaginatedResponse<ClsPedido>> GetHistoricoDePedidosPorPaginaAsync(QuerysDePedidos QueryDePedido)
    {
        string QueryDeFiltros = "";

        if (QueryDePedido.Status != null)
        {
            QueryDeFiltros += $"&Status={QueryDePedido.Status}";
        }

        if (!String.IsNullOrEmpty(QueryDePedido.Pesquisa))
        {
            QueryDeFiltros += $"&pesquisa={QueryDePedido.Pesquisa}";
        }

        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsPedido>>(
           $"pedidos/finalizados?limit={QueryDePedido.PageSize}&page={QueryDePedido.Page}{QueryDeFiltros}");

        return response!;
    }

    public async Task<ReturnApiRefatored<PedidoMesaDto>> GetMesaOcupada(int idDaMesa)
    {
        
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<PedidoMesaDto>>(
           $"pedidos/mesas/{idDaMesa}");

        return response!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> CreatePedido(ClsPedido Pedido)
    {
         var response = await _http.PostAsJsonAsync($"pedidos", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> FechaMesa(ClsPedido Pedido)
    {
         var response = await _http.PostAsJsonAsync($"pedidos/mesa", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<PedidoMesaDto>> CreatePedidoMesa(ClsPedido Pedido)
    {
         var response = await _http.PostAsJsonAsync($"pedidos", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<PedidoMesaDto>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoPreparando(ClsPedido Pedido)
    {
        var response = await _http.PutAsJsonAsync($"pedidos/preparando/{Pedido.Id}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoDespachadoEPronto(ClsPedido Pedido)
    {
        var response = await _http.PutAsJsonAsync($"pedidos/despachado/{Pedido.Id}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoFinalizadoo(ClsPedido Pedido)
    {
        var response = await _http.PutAsJsonAsync($"pedidos/finalizado/{Pedido.Id}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
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

    [JsonPropertyName("pesquisa")] public string Pesquisa { get; set; } = string.Empty;
}