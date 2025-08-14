using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services.FinanceroServices;

public class MetodosDePagMerchantService
{
    public HttpClient _HttpClient { get; set; }
    public MetodosDePagMerchantService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsMetodosDePagMerchant>?> GetMetodosAsync()
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsMetodosDePagMerchant>>("financeiro/metodos");
        return response?.Data.Lista ?? new List<ClsMetodosDePagMerchant>();
    }

    public async Task<ClsMetodosDePagMerchant?> GetMetodoAsync(int idDoMetodo)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsMetodosDePagMerchant>>($"financeiro/metodos/{idDoMetodo}");
        return response?.Data.Objeto ?? new ClsMetodosDePagMerchant();
    }

    public async Task<ReturnApiRefatored<ClsMetodosDePagMerchant>> AdicionarMetodoAsync(ClsMetodosDePagMerchant metodo)
    {
        var response = await _HttpClient.PostAsJsonAsync("financeiro/metodos", metodo);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMetodosDePagMerchant>>() ?? new ReturnApiRefatored<ClsMetodosDePagMerchant>();
    }

    public async Task<ReturnApiRefatored<ClsMetodosDePagMerchant>> UpdateMetodoAsync(ClsMetodosDePagMerchant metodo)
    {

        var response = await _HttpClient.PatchAsJsonAsync($"financeiro/metodos/{metodo.Id}", metodo);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMetodosDePagMerchant>>() ?? new ReturnApiRefatored<ClsMetodosDePagMerchant>();
    }

    public async Task<ReturnApiRefatored<ClsMetodosDePagMerchant>> DeleteMetodoAsync(ClsMetodosDePagMerchant metodo)
    {
        var response = await _HttpClient.DeleteAsync($"financeiro/metodos/{metodo.Id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMetodosDePagMerchant>>() ?? new ReturnApiRefatored<ClsMetodosDePagMerchant>();
    }
}


/*        var json = System.Text.Json.JsonSerializer.Serialize(
        metodo,
        new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true // deixa formatado (bonitinho)
        }
    );

        Console.WriteLine("JSON enviado:");
        Console.WriteLine(json);*/