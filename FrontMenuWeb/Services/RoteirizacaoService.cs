using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Roteirizacao;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontMenuWeb.Services;

public class RoteirizacaoService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RoteirizacaoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReturnApiRefatored<PedidoParaRota>> GetPedidosDeliveryDoCaixaAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("roteirizacao/pedidos-delivery-do-caixa");
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ReturnApiRefatored<PedidoParaRota>>(json, _jsonOptions); 

            return result ?? new ReturnApiRefatored<PedidoParaRota> { Status = "error", Messages = ["Retorno vazio da API"] };
        }
        catch (JsonException jex)
        {
            return new() { Status = "error", Messages = ["Falha ao ler JSON da API: " + jex.Message] };
        }
        catch (Exception ex)
        {
            return new() { Status = "error", Messages = ["Exceção: " + ex.Message] };
        }
    }

    public async Task<ReturnApiRefatored<RotaOtimizada>> OtimizarRotaAsync(OtimizarRotaDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("roteirizacao/otimizar", dto);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Roteirizacao] HTTP {(int)response.StatusCode} ({response.StatusCode}) — body: {json}");

            if (!response.IsSuccessStatusCode)
            {
                return new() { Status = "error", Messages = [$"HTTP {(int)response.StatusCode} {response.StatusCode}", TruncateBody(json)] };
            }

            return JsonSerializer.Deserialize<ReturnApiRefatored<RotaOtimizada>>(json, _jsonOptions)
                ?? new() { Status = "error", Messages = ["Retorno vazio da API"] };
        }
        catch (Exception ex)
        {
            return new() { Status = "error", Messages = [ex.Message] };
        }
    }

    private static string TruncateBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return "<vazio>";
        return body.Length > 400 ? body[..400] + "..." : body;
    }
}
