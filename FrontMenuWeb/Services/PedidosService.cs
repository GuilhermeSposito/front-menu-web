using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using FrontMenuWeb.Services.Fiscal;
using Microsoft.JSInterop;
using MudBlazor;
using Nextended.Core.Extensions;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core.Tokens;

namespace FrontMenuWeb.Services;

public class PedidosService
{
    private HttpClient _http;
    private readonly IConfiguration _configuration;

    public PedidosService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
    }

    public static Action<ClsPedido>? PedidoRecebido;
    public static Action<PedidoMesaDto>? PedidoMesaRecebido;
    public static Action<ClsMesasEComandas>? PedidoMesaFechada;
    public static Action<ClsPedido>? PedidoMudouEtapa;
    public static Action<ClsPedido>? PedidoMudouInfoAdicional;

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
    public static Task ReceiveInfoAdicionalDoPedido(string msg)
    {
        ClsPedido pedido = System.Text.Json.JsonSerializer.Deserialize<ClsPedido>(msg)!;

        PedidoMudouInfoAdicional?.Invoke(pedido);
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

        if(QueryDePedido.Imprimiu is not null)
        {
                QueryDeFiltros += $"&Imprimiu={QueryDePedido.Imprimiu}";

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

    public async Task<ReturnApiRefatored<ClsPedido>> CreatePedidoPublicAsync(ClsPedido Pedido,ClsMerchant Merchant,CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"pedidos/public/{Merchant.Id}", Pedido, cancellationToken);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ClsPedido?> GetPedidoById(int IdPedido, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"pedidos/{IdPedido}");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno?.Data?.Objeto;
    }

    public async Task<ClsPedido?> GetPedidoByIntegracaoId(string IdPedido, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"pedidos/ped-integracao/{IdPedido}");
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

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoImpresso(ClsPedido Pedido)
    {
        var response = await _http.PutAsJsonAsync($"pedidos/impresso/{Pedido.Id}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoPreparando(ClsPedido Pedido, string MechantSophosId)
    {  
        var response = await _http.PutAsJsonAsync($"pedidos/preparando/{Pedido.Id}/{MechantSophosId}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoDespachadoEPronto(ClsPedido Pedido)
    {
        var response = await _http.PutAsJsonAsync($"pedidos/despachado/{Pedido.Id}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }
    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoDespachadoEProntoPublic(ClsPedido Pedido, string? MerchantSophosId = null)
    {
        var QueryStringMerchantSophosId = !string.IsNullOrEmpty(MerchantSophosId) ? $"?merchant={MerchantSophosId}" : string.Empty;

        var response = await _http.PutAsJsonAsync($"pedidos/despachado/public/{Pedido.Id}{QueryStringMerchantSophosId}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoFinalizadoo(ClsPedido Pedido)
    {
        var response = await _http.PutAsJsonAsync($"pedidos/finalizado/{Pedido.Id}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoFinalizadoPublic(ClsPedido Pedido, string? MerchantSophosId = null)
    {
        var QueryStringMerchantSophosId = !string.IsNullOrEmpty(MerchantSophosId) ? $"?merchant={MerchantSophosId}" : string.Empty;

        var response = await _http.PutAsJsonAsync($"pedidos/finalizado/public/{Pedido.Id}{QueryStringMerchantSophosId}", Pedido);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> UpdatePedidoInfosAdicionaisOuStatus(UpdatePedidoInfosAdicionaisDto updatDto, ClsPedido pedido, string? MerchantSophosId = null)
    {
        var QueryStringMerchantSophosId = !string.IsNullOrEmpty(MerchantSophosId) ? $"?merchant={MerchantSophosId}" : string.Empty;

        var response = await _http.PutAsJsonAsync($"pedidos/info-adicional/{pedido.Id}{QueryStringMerchantSophosId}", updatDto);
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
        string ApiKey = _configuration["ApiKeyNest"] ?? string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("x-api-key", ApiKey);

        var response = await _http.SendAsync(request);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();
        return retorno!;
    }

    public async Task<ReturnApiRefatored<ClsPedido>> LimparMesa(ClsMesasEComandas mesa)
    {
        var response = await _http.DeleteAsync($"pedidos/cancelar/itens/{mesa.Id}");
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();
        return retorno!;
    }

    public async Task<ReturnApiRefatored<DtoEstastisticaPorProduto>> EstastisticaDeItensMaisVendidos(QueryDeHistoricoDePedidos? QueryDeHistorico = null)
    {
        string url = "pedidos/estatisticas/itens";

        if (QueryDeHistorico is not null)
        {
            var parametros = new List<string>();

            if (QueryDeHistorico.DataInicio != null)
                parametros.Add($"DataInicio={QueryDeHistorico.DataInicio:yyyy-MM-dd}");

            if (QueryDeHistorico.DataFinal != null)
                parametros.Add($"DataFinal={QueryDeHistorico.DataFinal:yyyy-MM-dd}");

            if(!string.IsNullOrEmpty(QueryDeHistorico.Descricao))
                parametros.Add($"Descricao={QueryDeHistorico.Descricao}");

            parametros.Add($"limit={QueryDeHistorico.Limit}");
            parametros.Add($"page={QueryDeHistorico.Page}");

            if (parametros.Count > 0)
                url += "?" + string.Join("&", parametros);
        }

        var response = await _http.GetAsync(url);
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
    public bool? Imprimiu = null;

    [JsonPropertyName("pesquisa")] public string Pesquisa { get; set; } = string.Empty;
}

public class QueryDeHistoricoDePedidos
{
    [JsonPropertyName("limit")] public int Limit { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("DataInicio")] public DateTime? DataInicio { get; set; }
    [JsonPropertyName("DataFinal")] public DateTime? DataFinal { get; set; }
    [JsonPropertyName("Descricao")] public string? Descricao { get; set; }

}