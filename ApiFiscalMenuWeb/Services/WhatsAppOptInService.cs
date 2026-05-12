using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Services;

// Proxy para os endpoints de opt-in/opt-out na NestAPI.
// Usa o HttpClient "ApiNestPublica" (já configurado com x-api-key).
public class WhatsAppOptInService
{
    private readonly IHttpClientFactory _factory;

    public WhatsAppOptInService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task RegistrarOptInAsync(string merchantId, string telefone, string via, string? ip = null)
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var payload = new { merchantId, telefone = Normalize(telefone), optedInVia = via, ipAddress = ip };
            var response = await client.PostAsJsonAsync("whatsapp-optins", payload);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"[OptIn] Falha ao registrar opt-in: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OptIn] Erro ao registrar opt-in: {ex.Message}");
        }
    }

    public async Task RegistrarOptOutAsync(string merchantId, string telefone)
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var payload = new { merchantId, telefone = Normalize(telefone) };
            var response = await client.PutAsJsonAsync("whatsapp-optins/optout", payload);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"[OptIn] Falha ao registrar opt-out: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OptIn] Erro ao registrar opt-out: {ex.Message}");
        }
    }

    public async Task<bool> EstaOptInAsync(string merchantId, string telefone)
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var tel = Uri.EscapeDataString(Normalize(telefone));
            var response = await client.GetAsync($"whatsapp-optins/status/{merchantId}/{tel}");
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<OptInStatusDto>();
            return result?.OptedIn ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OptIn] Erro ao verificar opt-in: {ex.Message}");
            return false;
        }
    }

    // Normaliza para E.164 sem '+': sempre com prefixo 55
    private static string Normalize(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (!digits.StartsWith("55") && digits.Length <= 11)
            return "55" + digits;
        return digits;
    }

    private class OptInStatusDto
    {
        [JsonPropertyName("optedIn")]
        public bool OptedIn { get; set; }
    }
}
