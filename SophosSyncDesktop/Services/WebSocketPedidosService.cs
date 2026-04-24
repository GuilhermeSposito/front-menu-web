using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Pedidos;
using SocketIOClient;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using System.Text.Json;

namespace SophosSyncDesktop.Services;

public class WebSocketPedidosService : IDisposable
{
    private readonly ImpressaoService _impressaoService;
    private readonly FilaDeImpressaoService _filaService;
    private SocketIOClient.SocketIO? _client;

    // Evita processar o mesmo evento WebSocket duas vezes seguidas (servidor emitindo em duplicata)
    private readonly Dictionary<int, DateTime> _pedidosProcessados = new();
    private readonly object _lockPedidos = new();
    private static readonly TimeSpan _janelaDeDedup = TimeSpan.FromSeconds(30);

    public bool EstaConectado { get; private set; }

    private const string BaseUrl = "https://sophos-erp.com.br"; //"http://localhost:3030";//"https://sophos-erp.com.br";

    public event Action<bool, string>? StatusChanged;

    public WebSocketPedidosService(ImpressaoService impressaoService, FilaDeImpressaoService filaService)
    {
        _impressaoService = impressaoService;
        _filaService = filaService;
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
                    { "token", AppState.Token ?? "" }
                },
                ExtraHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {AppState.Token ?? ""}" }
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
                            await ProcessarPedidoAsync(pedido, json).ConfigureAwait(false);
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
                        var pedidoMesa = JsonSerializer.Deserialize<PedidoMesaDto>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (pedidoMesa == null) return;


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

            _client.On("aviso-conta", response =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var json = response.GetValue<JsonElement>().GetRawText();
                        await _impressaoService.ImprimirFechamentoDeConta(json).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WS] Erro ao processar aviso-conta: {ex.Message}");
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
        if (pedido.CriadoPor == "SOPHOS") return;
        if (pedido.StatusPedido == "ABERTO") return;

        // Dedup: evita processar o mesmo evento WebSocket duas vezes em menos de 30s
        lock (_lockPedidos)
        {
            if (_pedidosProcessados.TryGetValue(pedido.Id, out var ultimaVez) &&
                (DateTime.Now - ultimaVez) < _janelaDeDedup)
                return;

            _pedidosProcessados[pedido.Id] = DateTime.Now;
        }

        SomService.TocarPedidoDelivery();

        using var db = new AppDbContext();
        var config = db.Impressoras.FirstOrDefault();
        if (config is null) return;

        bool deveImprimir = pedido.CriadoPor == "IFOOD"
            ? config.ImprimirIfood
            : config.ImprimirSophosCardapio;

        if (!deveImprimir) return;

        _filaService.Enfileirar(pedido, json, "WS");
    }

    public async Task DesconectarAsync()
    {
        if (_client is null) return;
        try { await _client.DisconnectAsync(); }
        catch { }
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
