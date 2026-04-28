using FrontMenuWeb.DTOS;
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

    // Memória de pedidos já impressos recentemente — impede re-impressão por WS + timer na mesma janela
    private readonly Dictionary<int, DateTime> _jaImpressos = new();
    private readonly Dictionary<int, DateTime> _itensJaImpressos = new();
    private static readonly TimeSpan _ttlImpresso = TimeSpan.FromMinutes(5);

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
            // Já foi impresso recentemente — descarta independente de WS ou timer
            if (_jaImpressos.TryGetValue(pedido.Id, out var imprestoEm) &&
                (DateTime.Now - imprestoEm) < _ttlImpresso)
                return false;

            // Já está na fila aguardando — descarta duplicata
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
                if (atualizado is not null && atualizado.Imprimiu)
                {
                    lock (_lock) { _jaImpressos[pedido.Id] = DateTime.Now; }
                    continue;
                }

                await _impressaoService.Imprimir(json, "SOPHOS").ConfigureAwait(false);
                await MarcarComoImpressoAsync(pedido).ConfigureAwait(false);

                lock (_lock)
                {
                    _jaImpressos[pedido.Id] = DateTime.Now;
                }

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
        lock (_lock)
        {
            _jaImpressos[pedido.Id] = DateTime.Now;
        }
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

    public async Task BuscarItensDeMesaNaoImpressosAsync()
    {
        try
        {
            using var http = CriarHttpClientComTimeout(10);
            var response = await http.GetAsync($"{ApiUrl}/pedidos/itens/mesa/nao-impressos");
            if (!response.IsSuccessStatusCode) return;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await response.Content.ReadFromJsonAsync<FrontMenuWeb.Models.ReturnApiRefatored<ItemMesaComMesaDto>>(options);
            var itens = result?.Data?.Lista ?? new List<ItemMesaComMesaDto>();

            var gruposPorMesa = itens
                .Where(i => i.Mesa != null)
                .GroupBy(i => i.Mesa!.Id)
                .ToList();

            foreach (var grupo in gruposPorMesa)
            {
                var mesaInfo = grupo.First().Mesa!;

                var itensParaImprimir = grupo.Where(i =>
                {
                    lock (_lock)
                    {
                        return !_itensJaImpressos.TryGetValue(i.Id, out var dt) ||
                               (DateTime.Now - dt) >= _ttlImpresso;
                    }
                }).ToList();

                if (!itensParaImprimir.Any()) continue;

                var pedidoMesa = new PedidoMesaDto
                {
                    IdentificacaoMesaOuComanda = mesaInfo.CodigoExterno,
                    Itens = itensParaImprimir.Cast<ItensPedido>().ToList()
                };

                string json = JsonSerializer.Serialize(pedidoMesa, options);
                await _impressaoService.ImprimirComanda(json, "TIMER").ConfigureAwait(false);

                foreach (var item in itensParaImprimir)
                {
                    await MarcarItemMesaComoImpressoAsync(item.Id);
                    lock (_lock) { _itensJaImpressos[item.Id] = DateTime.Now; }
                    LogLocalService.LogImpressao(item.Id.ToString(), "TIMER-MESA");
                }
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[Fila] Timeout ao buscar itens de mesa não impressos — sem internet.");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("[Fila] Sem conexão ao buscar itens de mesa não impressos.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fila] Erro ao buscar itens de mesa: {ex.Message}");
        }
    }

    private async Task MarcarItemMesaComoImpressoAsync(int itemId)
    {
        try
        {
            using var http = CriarHttpClientComTimeout(10);
            await http.PutAsync($"{ApiUrl}/pedidos/itens/mesa/impresso/{itemId}", null);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[Fila] Timeout ao marcar item {itemId} como impresso.");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"[Fila] Sem conexão ao marcar item {itemId} como impresso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fila] Erro ao marcar item {itemId} como impresso: {ex.Message}");
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

    private HttpClient CriarHttpClientComTimeout(int segundos)
    {
        var client = CriarHttpClient();
        client.Timeout = TimeSpan.FromSeconds(segundos);
        return client;
    }

    // DTOs locais para desserialização dos itens de mesa
    private class ItemMesaComMesaDto : ItensPedido
    {
        [System.Text.Json.Serialization.JsonPropertyName("Mesa")]
        public MesaInfoDto? Mesa { get; set; }
    }

    private class MesaInfoDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")] public int Id { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("CodigoExterno")] public int CodigoExterno { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("Apelido")] public string? Apelido { get; set; }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _canal.Writer.Complete();
    }
}
