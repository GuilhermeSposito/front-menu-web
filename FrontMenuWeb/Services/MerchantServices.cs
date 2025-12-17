using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services;

public class MerchantServices
{
    public HttpClient _HttpClient { get; set; }
    public MerchantServices(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<ClsMerchant> GetMerchantAsync()
    {
        var response = await _HttpClient.GetFromJsonAsync<ClsMerchant>("merchants/details");
        return response ?? new ClsMerchant();
    }

    public async Task<ReturnApiRefatored<ClsMerchant>> UpdateMerchantAsync(ClsMerchant merchant)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"merchants/update/{merchant.Id}", merchant);
        var updatedMerchant = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMerchant>>();
        return updatedMerchant ?? new ReturnApiRefatored<ClsMerchant>() { Status = "error", Messages = new List<string> { "Informações não alteradas" } };
    }

    public async Task<ReturnApiRefatored<ClsMerchant>> GetMerchantPublicAsync(string IdDoMerchant)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsMerchant>>($"merchants/details/public/{IdDoMerchant}");
        return response ?? new ReturnApiRefatored<ClsMerchant>();
    }
}
