using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Merchant;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

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

    #region Autorização e Autenticacao Region
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

    public async Task<ReturnApiRefatored<object>> AutenticarEmpresa(InformacoesParaAutenticarEmpresaIfoodDto Infos, string TokenNestAPi)
    {
        var HttpIfood = _factory.CreateClient("ApiIfood");
        FormUrlEncodedContent formDataToGetTheToken = new FormUrlEncodedContent(new[]
        {
              new KeyValuePair<string, string>("grantType", "authorization_code"),
              new KeyValuePair<string, string>("clientId", "7e476dce-79fa-4a7e-a605-aa2a1a40b803"),
              new KeyValuePair<string, string>("clientSecret", "z5086yxoeeblv5go12ag9ynk2i8oan36l0gca8y9vs0h66yrorjh2nccdmxpbxk955lb0j6wc7vdpb2i3416aqs8ja4xjhbw3u0"),
              new KeyValuePair<string, string>("authorizationCode", Infos.CodigoDeAutorizacaoEnviadoPeloIfood),
              new KeyValuePair<string, string>("authorizationCodeVerifier", Infos.VerificadorDoCodigo)
        });

        var response = await HttpIfood.PostAsync("/authentication/v1.0/oauth/token", formDataToGetTheToken);
        var result = await response.Content.ReadFromJsonAsync<InformacoesDoTokenRetornadaPeloIfoodDto>();

        if (result is not null && result.AccessToken is not null && result.RefreshToken is not null)
        {

            var NovaEmpresa = new ClsEmpresaIfood
            {
                NomeEmpresa = Infos.NomeEmpresa,
                MerchantIdIfood = Infos.MerchantIdIfood,
                AccessTokenIfood = result.AccessToken,
                RefreshTokenIfood = result.RefreshToken,
                VenceTokenIfood = DateTime.Now.AddSeconds(result.ExpiresIn - 3600),
                Ativo = true
            };

            bool AdicionouEmpresa = await _nestApiService.EditarEAdicionarEmpresaIfood(NovaEmpresa, TokenNestAPi);

            if (AdicionouEmpresa)
                return new ReturnApiRefatored<object>
                {
                    Status = response.IsSuccessStatusCode ? "success" : "error",
                    Messages = new List<string> { "Empresa Autenticada com Sucesso!" },
                    Data = new Data<object> { ObjetoWhenWriting = result, Messages = new List<string> { "Empresa Autenticada com sucesso!" } }
                };
        }


        return new ReturnApiRefatored<object>
        {
            Status = "error",
            Messages = new List<string> { "Erro Ao obter Resposta do Ifood" }
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


