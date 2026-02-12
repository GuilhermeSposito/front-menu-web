using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using FrontMenuWeb.Services.Fiscal;
using Microsoft.JSInterop;
using MudBlazor;
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
    public static event Action<ClsMesasEComandas>? PedidoMesaFechada;
    public static event Action<ClsPedido>? PedidoMudouEtapa;

    [JSInvokable]
    public static Task ReceivePedido(string msg)
    {
        ClsPedido pedido = System.Text.Json.JsonSerializer.Deserialize<ClsPedido>(msg)!;

        PedidoRecebido?.Invoke(pedido);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public static Task ReceiveEtapaDoPedido(string msg)
    {
        ClsPedido pedido = System.Text.Json.JsonSerializer.Deserialize<ClsPedido>(msg)!;

        PedidoMudouEtapa?.Invoke(pedido);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public static Task ReceivePedidoMesa(string msg)
    {
        PedidoMesaDto pedido = System.Text.Json.JsonSerializer.Deserialize<PedidoMesaDto>(msg)!;

        PedidoMesaRecebido?.Invoke(pedido);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public static Task ReceivePedidoMesaFechada(string msg)
    {
        ClsMesasEComandas mesa = System.Text.Json.JsonSerializer.Deserialize<ClsMesasEComandas>(msg)!;

        PedidoMesaFechada?.Invoke(mesa);
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

        if (QueryDePedido.TipoPedido is not null)
        {
            if (QueryDePedido.TipoPedido != TiposDePedido.TODOS)
                QueryDeFiltros += $"&TipoPedido={QueryDePedido.TipoPedido}";
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

        if (QueryDePedido.DataCriadoEmInicio is not null && QueryDePedido.DataCriadoEmFinal is not null)
            QueryDeFiltros += $"&DataCriadoEmInicio={QueryDePedido.DataCriadoEmInicio?.ToString("yyyy-MM-ddTHH:mm:ssZ")}&DataCriadoEmFinal={QueryDePedido.DataCriadoEmFinal?.ToString("yyyy-MM-ddTHH:mm:ssZ")}";

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

    public async Task<ReturnApiRefatored<ClsPedido>> CreatePedido(ClsPedido Pedido, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"pedidos", Pedido, cancellationToken);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ClsPedido?> GetPedidoById(int IdPedido, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"pedidos/{IdPedido}");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno?.Data?.Objeto;
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

    public async Task<ReturnApiRefatored<PedidoMesaDto>> CreatePedidoMesaPublic(string MerchantID, ClsPedido Pedido)
    {
        var response = await _http.PostAsJsonAsync($"pedidos/mesa/public/{MerchantID}", Pedido);
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

    //Tivemos que colocar aqui por causa do problema de não ter o token certo no NFservice, e sim o token da api fiscal
    public async Task<PaginatedResponse<NFEmitidasDto>> ConsultaNFeEmitidas(DateTime? DataCriadoEmInicio, DateTime? DataCriadoEmFinal, int page = 1, int limit = 10)
    {
        var QueryDeFiltros = "";

        if (DataCriadoEmInicio is not null && DataCriadoEmFinal is not null)
            QueryDeFiltros += $"&DataCriadoEmInicio={DataCriadoEmInicio?.ToString("yyyy-MM-ddTHH:mm:ssZ")}&DataCriadoEmFinal={DataCriadoEmFinal?.ToString("yyyy-MM-ddTHH:mm:ssZ")}";

        var httpResponse = await _http.GetAsync($"nfs?page={page}&limit={limit}{QueryDeFiltros}");

        var retorno = await httpResponse.Content.ReadFromJsonAsync<PaginatedResponse<NFEmitidasDto>>();
        return retorno ?? new PaginatedResponse<NFEmitidasDto>();
    }

    public async Task<ReturnApiRefatored<NFEmitidasDto>> DeleteRegistroDaNF(int id)
    {
        var httpResponse = await _http.DeleteAsync($"nfs/{id}");
        var retorno = await httpResponse.Content.ReadFromJsonAsync<ReturnApiRefatored<NFEmitidasDto>>();
        return retorno ?? new ReturnApiRefatored<NFEmitidasDto>();
    }

    public async Task<ReturnApiRefatored<ClsPedido>> CancelarPedido(ClsPedido Pedido, bool ePedidoDeCaixaFechado = false)
    {
        var url = ePedidoDeCaixaFechado ? $"pedidos/cancelar/fechado/{Pedido.Id}" : $"pedidos/cancelar/{Pedido.Id}";

        var response = await _http.DeleteAsync(url);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();
        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> LimparMesa(ClsMesasEComandas mesa) 
    {
        var response = await _http.DeleteAsync($"pedidos/cancelar/itens/{mesa.Id}");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();
        return retorno!;
    }

    public async Task<ReturnApiRefatored<DtoEstastisticaPorProduto>> EstastisticaDeItensMaisVendidos()
    {
        var response = await _http.GetAsync($"pedidos/estatisticas/itens");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<DtoEstastisticaPorProduto>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<DtoEstastisticaPorProduto>> EstastisticaDeGruposMaisVendidos()
    {
        var response = await _http.GetAsync($"pedidos/estatisticas/grupos");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<DtoEstastisticaPorProduto>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<DtoEstastisticaPorProduto>> EstastisticaDeFormasMaisRecebidas()
    {
        var response = await _http.GetAsync($"pedidos/estatisticas/formasderecebimento");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<DtoEstastisticaPorProduto>>();

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