using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using System.Net.Http;
using System.Net.Http.Json;
using ZstdSharp.Unsafe;

namespace FrontMenuWeb.Services.FinanceroServices;

public class FormasDeRecebimentoService
{
    public HttpClient HttpClient { get; set; }
    public FormasDeRecebimentoService(HttpClient http)
    {
        HttpClient = http;
    }

    public async Task<List<ClsFormaDeRecebimento>?> GetFormasDeRecebimentoAsync()
    {
        var response = await HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>("financeiro/formas-recebimento");
        return response?.Data.Lista ?? new List<ClsFormaDeRecebimento>();
    }

    public async Task<ClsFormaDeRecebimento?> GetFormaDeRecebimentoAsync(int IdDaForma)
    {
        var response = await HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>($"financeiro/formas-recebimento/{IdDaForma}");
        return response?.Data.Objeto ?? new ClsFormaDeRecebimento();
    }

    public async Task<ReturnApiRefatored<ClsFormaDeRecebimento>> AdicionarFormaDeRecebimentoAsync(ClsFormaDeRecebimento formaDeRecebimento)
    {
        var response = await HttpClient.PostAsJsonAsync("financeiro/formas-recebimento", formaDeRecebimento);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>() ?? new ReturnApiRefatored<ClsFormaDeRecebimento>();
    }

}
