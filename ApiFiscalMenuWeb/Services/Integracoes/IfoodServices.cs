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

    #region Pooling Region
    public async Task Pooling(string MerchantId, string TokenNestApi)
    {
        ClsMerchant? Merchant = await _nestApiService.GetMerchantFromNestApi(TokenNestApi);
        if (Merchant is null)
            throw new Exception("Não Foi possivel obter acesso as informações do estabelecimento!");


    }
    #endregion
}
