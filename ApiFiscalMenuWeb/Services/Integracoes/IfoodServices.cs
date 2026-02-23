using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using Microsoft.AspNetCore.Mvc;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class IfoodServices
{
    #region Propriedades
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiService;

    public IfoodServices(IHttpClientFactory factory, NestApiServices nestApiService)
    {
        _factory = factory;
        _nestApiService = nestApiService;
    }
    #endregion

    #region Autorização Region
    public async Task<ReturnApiRefatored<object>> GetAutorizationCode()
    {
        var HttpIfood = _factory.CreateClient("ApiIfood");
        FormUrlEncodedContent formData = new FormUrlEncodedContent(new[]
        {
              new KeyValuePair<string, string>("clientId", "7e476dce-79fa-4a7e-a605-aa2a1a40b803")
        });
        var response = await HttpIfood.PostAsync("/authentication/v1.0/oauth/userCode", formData);
        var result = await response.Content.ReadFromJsonAsync<UserCodeReturnFromAPIIfoodDto>();

        return new ReturnApiRefatored<object>
        {
            Status = response.IsSuccessStatusCode ? "success" : "error",
            Messages = new List<string> { result.VerificationUrlComplete ?? "" },
            Data = new Data<object> { ObjetoWhenWriting = result }
        };
    }

    #endregion

    #region Pooling Region
    public async Task Pooling(string MerchantId, string TokenNestApi)
    {
        ClsMerchant? Merchant = await _nestApiService.GetMerchantFromNestApi(TokenNestApi);
        if (Merchant is null)
            throw new Exception("Não Foi possivel obter acesso as informações do estabelecimento!");


    }
    #endregion
}
