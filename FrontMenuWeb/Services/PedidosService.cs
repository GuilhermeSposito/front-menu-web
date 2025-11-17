using FrontMenuWeb.Models;
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


    [JSInvokable]
    public static Task ReceivePedido(string msg)
    {
        ClsPedido pedido = System.Text.Json.JsonSerializer.Deserialize<ClsPedido>(msg)!;

        PedidoRecebido?.Invoke(pedido);
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

        Console.WriteLine($"pedidos?limit={QueryDePedido.PageSize}&page={QueryDePedido.Page}{QueryDeFiltros}");

        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsPedido>>(
           $"pedidos?limit={QueryDePedido.PageSize}&page={QueryDePedido.Page}{QueryDeFiltros}");

        return response!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> CreatePedido(ClsPedido Pedido)
    {
        var response = await _http.PostAsJsonAsync($"pedidos", Pedido);
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