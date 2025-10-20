namespace FrontMenuWeb.Services;

// Services/WebSocketService.cs
using SocketIOClient;

public class WebSocketService
{
    private SocketIO? _socket;
    private readonly string _baseUrl = "https://syslogicadev.com:3030"; // 🚨 SUA URL

    public event Action<string>? OnMessageReceived;
    public event Action? OnConnected;
    public event Action<string>? OnError;
    public event Action? OnDisconnected;

    public async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine($"🚀 Tentando conectar em: {_baseUrl}");

            _socket = new SocketIO(_baseUrl, new SocketIOOptions
            {
                Reconnection = false,
                ReconnectionAttempts = 5,
                ReconnectionDelay = 1000,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ExtraHeaders = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJNZXJjaGFudCI6eyJpZCI6ImQ2Nzc4ODU5LTE2ZmItNGYyMC1hNDU2LTllNmRiNTNjMWUwNSIsImVtYWlsIjoib2RhaXJAcGlhc3NhLmNvbS5iciIsInJhemFvU29jaWFsIjoiVkVORFJVU0NPTE8gQkVSVEkgRSBDSUEgTFREQSIsIkltYWdlbUxvZ28iOiJodHRwczovL2V4ZW1wbG8uY29tL2xvZ28ucG5nIiwiTm9tZUZhbnRhc2lhIjoiUkVTVEFVUkFOVEUgUElBU1NBIiwiZW5kZXJlY29zX21lcmNoYW50IjpbXSwiZG9jdW1lbnRvcyI6W3siaWQiOiJhMGI0ZTc1YS01YzFkLTRhYzMtOTU1My0xNzcxZTc3OGJjMGEiLCJjbnBqIjoiMzUuMTkwLjEyNy8wMDAxLTAzIiwiaW5zY3JpY2FvRXN0YWR1YWwiOiIiLCJjbmFlIjoiIiwiaW5zY3JpY2FvTXVuaWNpcGFsIjoiIn1dLCJ0ZWxlZm9uZXMiOltdLCJtYXJjYURlcGFydGFtZW50byI6bnVsbCwibGVnZW5kYURvVm9sdW1hIjpudWxsLCJhdGl2byI6dHJ1ZX0sImlzQWRtaW4iOmZhbHNlLCJpYXQiOjE3NjA5NjU2MjksImV4cCI6MTc2MDk5ODAyOX0.M968EaA44XmJ82hFh0DI52zO42r83YTYNe4OOKjciAk" }
                }
            });


            // 🚨 EVENTOS ANTES DA CONEXÃO
            _socket.OnConnected += async (socket, e) =>
            {
                Console.WriteLine("✅ CONECTADO AO WEBSOCKET!");
                OnConnected?.Invoke();
                await _socket.EmitAsync("registrar-merchant");
                await Task.CompletedTask;
            };

            _socket.OnDisconnected += async (socket, e) =>
            {
                Console.WriteLine("🔌 DESCONECTADO");
                OnDisconnected?.Invoke();
                await Task.CompletedTask;
            };


            _socket.On("registrado", response =>
            {
                var message = response.GetValue<string>();
                Console.WriteLine($"📨 'registrar-merchant': {message}");
                OnMessageReceived?.Invoke($"[registrar-merchant] {message}");
            });

            _socket.On("pedido-recebido", response =>
            {
                var message = response.GetValue<string>();
                Console.WriteLine($"📨 'pedido-recebido': {message}");
                OnMessageReceived?.Invoke($"[pedido-recebido] {message}");
            });

            // 🚨 EVENTO GENÉRICO
            _socket.OnAny((key, data) =>
            {
                Console.WriteLine($"📨 Evento '{key}': {data}");
                OnMessageReceived?.Invoke($"[{key}] {data}");
            });

            _socket.OnError += (socket, e) =>
            {
                Console.WriteLine($"❌ ERRO WEBSOCKET: {e}");
                OnError?.Invoke(e);
            };

            // 🚨 CONECTA
            await _socket.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 EXCEÇÃO: {ex.ToString()}");
            OnError?.Invoke($"Falha na conexão: {ex.Message}");
            throw; // Re-throw para componente capturar
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_socket?.Connected == true)
        {
            await _socket.EmitAsync("message", message);
            Console.WriteLine($"📤 Enviado: {message}");
        }
    }

    public async Task JoinRoomAsync(string room)
    {
        if (_socket?.Connected == true)
        {
            await _socket.EmitAsync("registrar-merchant", new { room });
        }
    }

    public async Task DisconnectAsync()
    {
        if (_socket != null)
        {
            await _socket.DisconnectAsync();
            _socket.Dispose();
            _socket = null;
            Console.WriteLine("🔌 Desconectado manualmente");
        }
    }

    public bool IsConnected => _socket?.Connected == true;

    public string GetCurrentUrl() => _baseUrl;
}