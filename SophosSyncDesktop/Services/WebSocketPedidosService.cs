using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using SocketIOClient;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SophosSyncDesktop.Services;

public class WebSocketPedidosService : IDisposable
{
    private readonly ImpressaoService _impressaoService;
    private SocketIOClient.SocketIO? _client;

    // Evita impressão duplicada: guarda IDs de pedidos em processamento ou recentemente impressos
    private readonly Dictionary<int, DateTime> _pedidosProcessados = new();
    private readonly object _lockPedidos = new();
    private static readonly TimeSpan _janelaDeDedup = TimeSpan.FromSeconds(30);

    public bool EstaConectado { get; private set; }

    private const string BaseUrl = "https://sophos-erp.com.br";
    private const string ApiUrl = "https://sophos-erp.com.br/api/v1";

    /// <summary>
    /// Disparado quando o status da conexão muda.
    /// bool = conectado, string = mensagem descritiva.
    /// </summary>
    public event Action<bool, string>? StatusChanged;

    public WebSocketPedidosService(ImpressaoService impressaoService)
    {
        _impressaoService = impressaoService;
    }

    public async Task ConectarAsync()
    {

        if (_client != null) return;

        try
        {
            _client = new SocketIOClient.SocketIO(BaseUrl, new SocketIOOptions
            {
                Path = "/socket.io/",
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Auth = new Dictionary<string, string>
                {
                    { "token", SophosSyncDesktop.Models.AppState.Token ?? "" }
                },
                ExtraHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {SophosSyncDesktop.Models.AppState.Token ?? ""}" }
                },
                ReconnectionAttempts = int.MaxValue,
                ReconnectionDelay = 5000
            });

            _client.OnConnected += async (s, e) =>
            {
                EstaConectado = true;
                StatusChanged?.Invoke(true, "Conectado");
                await _client.EmitAsync("registrar-merchant");
            };

            _client.OnDisconnected += (s, e) =>
            {
                EstaConectado = false;
                StatusChanged?.Invoke(false, "Desconectado — reconectando...");
            };

            _client.OnError += (s, e) =>
            {
                EstaConectado = false;
                StatusChanged?.Invoke(false, "Erro na conexão");
                Console.WriteLine($"[WS] Erro: {e}");
            };

            _client.On("pedido-recebido", response =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var json = response.GetValue<JsonElement>().GetRawText();
                        var pedido = JsonSerializer.Deserialize<ClsPedido>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (pedido is not null && pedido.CriadoPor != "SOPHOS")
                        {         
                            await ProcessarPedidoAsync(pedido, json).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WS] Erro ao processar pedido-recebido: {ex.Message}");
                    }
                });
            });

            _client.On("pedido-recebido-mesa", response =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var json = response.GetValue<JsonElement>().GetRawText();

                        SomService.TocarPedidoMesa();

                        using var db = new AppDbContext();
                        var config = db.Impressoras.FirstOrDefault();
                        if (config is null || !config.ImprimirComandaMesa) return;

                        await _impressaoService.ImprimirComanda(json, "SOPHOS", EMesa: true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WS] Erro ao processar pedido-recebido-mesa: {ex.Message}");
                    }
                });
            });

            await _client.ConnectAsync();
        }
        catch (Exception ex)
        {
            EstaConectado = false;
            StatusChanged?.Invoke(false, "Falha ao conectar");
            Console.WriteLine($"[WS] Falha ao conectar: {ex.Message}");
        }
    }

    private async Task ProcessarPedidoAsync(ClsPedido pedido, string json)
    {
        // Apenas pedidos de integrações externas (IFOOD, SOPHOS CARDAPIO)
        if (pedido.CriadoPor == "SOPHOS")
            return;

        // Pedido ainda não aceito — aguarda aceitação
        if (pedido.StatusPedido == "ABERTO")
            return;

        // Bloqueia processamento duplicado: simultâneo (race condition) ou sequencial (API emitindo duas vezes)
        lock (_lockPedidos)
        {
            if (_pedidosProcessados.TryGetValue(pedido.Id, out var ultimaVez) &&
                (DateTime.Now - ultimaVez) < _janelaDeDedup)
                return;

            _pedidosProcessados[pedido.Id] = DateTime.Now;
        }

        SomService.TocarPedidoDelivery();

        // Verifica preferências no banco local
        using var db = new AppDbContext();
        var config = db.Impressoras.FirstOrDefault();
        if (config is null) return;

        bool deveImprimir = pedido.CriadoPor == "IFOOD"
            ? config.ImprimirIfood
            : config.ImprimirSophosCardapio;

        if (!deveImprimir) return;

        // Consulta API para confirmar que não foi impresso ainda
        var pedidoAtualizado = await BuscarPedidoAsync(pedido.Id).ConfigureAwait(false);
        if (pedidoAtualizado is null || pedidoAtualizado.Imprimiu) return;

        // Imprime
        await _impressaoService.Imprimir(json, "SOPHOS").ConfigureAwait(false);

        // Marca como impresso na API
        await MarcarComoImpressoAsync(pedido).ConfigureAwait(false);
    }

    private async Task<ClsPedido?> BuscarPedidoAsync(int id)
    {
        try
        {
            using var http = CriarHttpClient();
            var response = await http.GetAsync($"{ApiUrl}/pedidos/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content
                .ReadFromJsonAsync<ReturnApiRefatored<ClsPedido>>();

            return result?.Data?.Objeto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WS] Erro ao buscar pedido {id}: {ex.Message}");
            return null;
        }
    }

    private async Task MarcarComoImpressoAsync(ClsPedido pedido)
    {
        try
        {
            using var http = CriarHttpClient();
            var retorno = await http.PutAsJsonAsync($"{ApiUrl}/pedidos/impresso/{pedido.Id}", pedido);

            var jsondoRetorno = await retorno.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WS] Erro ao marcar pedido {pedido.Id} como impresso: {ex.Message}");
        }
    }

    public async Task BuscarPedidosNaoImpressosAsync()
    {
        try
        {
            using var http = CriarHttpClient();
            var response = await http.GetAsync($"{ApiUrl}/pedidos?Imprimiu=false");

            if (!response.IsSuccessStatusCode)
                return;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResponse<ClsPedido>>(options);

            var Pedidos = result?.Data ?? new List<ClsPedido>();

            if (Pedidos.Count == 0)
                return;

            foreach (var pedido in Pedidos)
            {
                await ProcessarPedidoAsync(pedido, JsonSerializer.Serialize(pedido, options)).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WS] Erro ao buscar pedidos não impressos: {ex.Message}");
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

    public async Task DesconectarAsync()
    {
        if (_client is null) return;
        try
        {
            await _client.DisconnectAsync();
        }
        catch { /* ignora erros no shutdown */ }
        finally
        {
            _client.Dispose();
            _client = null;
        }
    }

    public void Dispose()
    {
        if (_client is null) return;
        try { _client.Dispose(); } catch { }
        _client = null;
    }
}
