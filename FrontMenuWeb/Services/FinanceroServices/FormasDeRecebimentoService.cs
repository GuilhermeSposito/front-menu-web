using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ZstdSharp.Unsafe;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services.FinanceroServices;

public class FormasDeRecebimentoService
{
    public HttpClient HttpClient { get; set; }
    public FormasDeRecebimentoService(HttpClient http)
    {
        HttpClient = http;
    }

    public async Task<List<ClsFormaDeRecebimento>> GetFormasDeRecebimentoAsync()
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

    public async Task<ReturnApiRefatored<ClsFormaDeRecebimento>> AtualizarFormaDeRecebimentoAsync(ClsFormaDeRecebimento formaDeRecebimento)
    {
        var response = await HttpClient.PatchAsJsonAsync($"financeiro/formas-recebimento/update/{formaDeRecebimento.Id}", formaDeRecebimento);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>() ?? new ReturnApiRefatored<ClsFormaDeRecebimento>();
    }

    public async Task<ReturnApiRefatored<ClsFormaDeRecebimento>> DeletarFormaDeRecebimentoAsync(int IdDaForma)
    {
        var response = await HttpClient.DeleteAsync($"financeiro/formas-recebimento/delete/{IdDaForma}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>() ?? new ReturnApiRefatored<ClsFormaDeRecebimento>();
    }

    public async Task<ReturnApiRefatored<ClsFormaDeRecebimento>> AdicionarContaAFormaDeRecebimento(int idDaForma, int idDaConta) {

        var conta = new AddContaAFormaDePagamentoRequest() { contasIds = { idDaConta } };

        var response = await HttpClient.PatchAsJsonAsync($"financeiro/formas-recebimento/add-conta/{idDaForma}", conta);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>() ?? new ReturnApiRefatored<ClsFormaDeRecebimento>();
    }

    public async Task<ReturnApiRefatored<ClsFormaDeRecebimento>> RemoverContaDaFormaDeRecebimento( int idDaConta)
    {   
        var response = await HttpClient.DeleteAsync($"financeiro/formas-recebimento/delete-conta/{idDaConta}"); //Remove apenas a conta da forma de recebimento, não deleta a conta
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsFormaDeRecebimento>>() ?? new ReturnApiRefatored<ClsFormaDeRecebimento>();
    }

}


class AddContaAFormaDePagamentoRequest
{
    [JsonPropertyName("contasIds")]public List<int> contasIds { get; set; } = new List<int>();
}
