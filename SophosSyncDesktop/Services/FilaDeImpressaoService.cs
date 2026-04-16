using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;

namespace SophosSyncDesktop.Services;

public class FilaDeImpressaoService : IDisposable
{
    private readonly ImpressaoService _impressaoService;
    private readonly Channel<(ClsPedido Pedido, string Json, string Origem)> _canal;
    private readonly HashSet<int> _pedidosNaFila = new();

    // Gate de impressão: quem adicionar primeiro imprime — o outro descarta
    private readonly HashSet<int> _pedidosEmImpressao = new();

    private readonly object _lock = new();
    private readonly CancellationTokenSource _cts = new();

    private const string ApiUrl = "https://sophos-erp.com.br/api/v1";

    public FilaDeImpressaoService(ImpressaoService impressaoService)
    {
        _impressaoService = impressaoService;
        _canal = Channel.CreateUnbounded<(ClsPedido, string, string)>(new UnboundedChannelOptions { SingleReader = true });
        _ = Task.Run(() => ProcessarFilaAsync(_cts.Token));
    }

    public bool Enfileirar(ClsPedido pedido, string json, string origem)
    {
        lock (_lock)
        {
            if (!_pedidosNaFila.Add(pedido.Id)) return false;
        }
        _canal.Writer.TryWrite((pedido, json, origem));
        return true;
    }

    /// <summary>
    /// Tenta reservar o slot de impressão para o Watcher (impressão manual).
    /// Retorna false se o worker já está imprimindo este pedido.
    /// Deve ser chamado ANTES de imprimir.
    /// </summary>
    public bool TentarReservarImpressao(int pedidoId)
    {
        lock (_lock)
        {
            return _pedidosEmImpressao.Add(pedidoId);
        }
    }

    /// <summary>
    /// Libera o slot após o Watcher terminar de imprimir.
    /// </summary>
    public void LiberarImpressao(int pedidoId)
    {
        lock (_lock)
        {
            _pedidosEmImpressao.Remove(pedidoId);
        }
    }

    public async Task BuscarPedidosNaoImpressosAsync()
    {
        try
        {
            using var http = CriarHttpClient();
            var response = await http.GetAsync($"{ApiUrl}/pedidos?Imprimiu=false");
            if (!response.IsSuccessStatusCode) return;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<ClsPedido>>(options);
            var pedidos = result?.Data ?? new List<ClsPedido>();

            foreach (var pedido in pedidos.Where(p => p.CriadoPor != "SOPHOS"))
            {
                using var db = new AppDbContext();
                var config = db.Impressoras.FirstOrDefault();
                if (config is null) continue;

                bool deveImprimir = pedido.CriadoPor == "IFOOD"
                    ? config.ImprimirIfood
                    : config.ImprimirSophosCardapio;

                if (deveImprimir)
                    Enfileirar(pedido, JsonSerializer.Serialize(pedido, options), "TIMER");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fila] Erro ao buscar pedidos não impressos: {ex.Message}");
        }
    }

    private async Task ProcessarFilaAsync(CancellationToken ct)
    {
        await foreach (var (pedido, json, origem) in _canal.Reader.ReadAllAsync(ct))
        {
            bool slotAdquirido = false;
            try
            {
                // Adquire o slot PRIMEIRO — se o Watcher já reservou, descarta sem nem chamar a API
                lock (_lock)
                {
                    slotAdquirido = _pedidosEmImpressao.Add(pedido.Id);
                }
                if (!slotAdquirido) continue;

                // Só então verifica a API — se já foi impresso por outro caminho, libera e pula
                var atualizado = await BuscarPedidoAsync(pedido.Id).ConfigureAwait(false);
                if (atualizado is not null && atualizado.Imprimiu) continue;

                await _impressaoService.Imprimir(json, "SOPHOS").ConfigureAwait(false);
                await MarcarComoImpressoAsync(pedido).ConfigureAwait(false);
                LogLocalService.LogImpressao(pedido.DisplayId ?? pedido.Id.ToString(), origem);
            }
            catch (Exception ex)
            {
                LogLocalService.LogErro(pedido.DisplayId ?? pedido.Id.ToString(), origem, ex.Message);
                Console.WriteLine($"[Fila] Erro ao imprimir pedido {pedido.Id}: {ex.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _pedidosNaFila.Remove(pedido.Id);
                    if (slotAdquirido) _pedidosEmImpressao.Remove(pedido.Id);
                }
            }
        }
    }

    private async Task<ClsPedido?> BuscarPedidoAsync(int id)
    {
        try
        {
            using var http = CriarHttpClient();
            var response = await http.GetAsync($"{ApiUrl}/pedidos/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<FrontMenuWeb.Models.ReturnApiRefatored<ClsPedido>>();
            return result?.Data?.Objeto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fila] Erro ao buscar pedido {id}: {ex.Message}");
            return null;
        }
    }

    public async Task MarcarComoImpressoPublicAsync(ClsPedido pedido)
    {
        await MarcarComoImpressoAsync(pedido);
    }

    private async Task MarcarComoImpressoAsync(ClsPedido pedido)
    {
        try
        {
            using var http = CriarHttpClient();
            await http.PutAsJsonAsync($"{ApiUrl}/pedidos/impresso/{pedido.Id}", pedido);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fila] Erro ao marcar pedido {pedido.Id} como impresso: {ex.Message}");
        }
    }

    private HttpClient CriarHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", SophosSyncDesktop.Models.AppState.Token ?? "");
        SophosSyncDesktop.Models.AppState.AddHmacHeaders(client);
        return client;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _canal.Writer.Complete();
    }
}
