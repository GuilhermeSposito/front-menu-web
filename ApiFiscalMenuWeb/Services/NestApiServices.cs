using FrontMenuWeb.Components.Modais.ModaisDeCadastros.EmpresaIfood;
using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiFiscalMenuWeb.Services;

public class NestApiServices
{
    #region Props
    private readonly IHttpClientFactory _factory;

    public NestApiServices(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    #endregion

    public async Task<ClsMerchant?> GetMerchantFromNestApi(string token)
    {
        try
        {
            var client = _factory.CreateClient("ApiAutorizada");
            AdicionaTokenNaRequisicao(client, token);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync("merchants/details", cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("A requisição para 'merchants/details' excedeu o tempo limite.");
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
                return null;

            var merchant = JsonSerializer.Deserialize<ClsMerchant>(content);

            return merchant;
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException("A requisição para 'merchants/details' excedeu o tempo limite.");
        }
        catch (Exception ex)
        {
            return null;
        }

    }

    private void AdicionaTokenNaRequisicao(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<bool> EditarEAdicionarEmpresaIfood(ClsEmpresaIfood empresa, string TokenNestApi)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        AdicionaTokenNaRequisicao(client, TokenNestApi);
        var EmpresaServiceNest = new EmpresaIfoodService(client, _factory);

        var RetornoDoCreate = await EmpresaServiceNest.CreateEmpresa(empresa);

        return RetornoDoCreate.Status == "success" ? true : false;
    }

}
