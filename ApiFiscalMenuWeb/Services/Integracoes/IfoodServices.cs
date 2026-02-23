using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Merchant;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
using Unimake.MessageBroker.Primitives.Contract.Response;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class IfoodServices
{
    #region Propriedades
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiService;
    private readonly ILogger<IfoodServices> _logger;

    public IfoodServices(IHttpClientFactory factory, NestApiServices nestApiService, ILogger<IfoodServices> logger)
    {
        _factory = factory;
        _nestApiService = nestApiService;
        _logger = logger;
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
    public async Task<ReturnApiRefatored<object>> Polling(string TokenNestApi)
    {
        try
        {
            ClsMerchant? Merchant = await _nestApiService.GetMerchantFromNestApi(TokenNestApi);
            if (Merchant is null)
                throw new Exception("Não Foi possivel obter acesso as informações do estabelecimento!");

            List<string> Messages = new List<string>();
            List<PollingIfoodDto> PollingsToAcknowledge = new List<PollingIfoodDto>();
            if (Merchant.EmpresasIfood.Count() > 0)
            {
                var IfoodClient = _factory.CreateClient("ApiIfood");
                foreach (var merchantsIfood in Merchant.EmpresasIfood)
                {
                    string AccessToken = merchantsIfood.AccessTokenIfood;
                    AdicionaTokenNaRequisicao(IfoodClient, AccessToken);

                    var PoolingResponse = await IfoodClient.GetAsync("/events/v1.0/events:polling");
                    if (PoolingResponse.IsSuccessStatusCode)
                    {
                        //Aqui você pode processar a resposta do pooling, por exemplo, lendo os pedidos e salvando no banco de dados
                        int statusCode = (int)PoolingResponse.StatusCode;
                        if (statusCode != 200)
                            continue;

                        List<PollingIfoodDto> Poolings = JsonConvert.DeserializeObject<List<PollingIfoodDto>>(await PoolingResponse.Content.ReadAsStringAsync()) ?? new List<PollingIfoodDto>();
                        foreach (var P in Poolings)
                        {
                            switch (P.Code)
                            {
                                case "PLC": //caso entre aqui é porque é um novo pedido     
                                    var Pedido = await GetPedido(P.OrderId, IfoodClient);
                                    Messages.Add($"{Pedido}");
                                    PollingsToAcknowledge.Add(P);
                                    break;
                                case "CFM":
                                    break;
                                case "CAR":
                                    break;
                                case "CAN":
                                    break;
                                case "CANF":
                                    break;
                                case "CON":
                                    break;
                                case "DDCR":
                                    break;
                                case "DSP":
                                    break;
                                case "RDR":
                                    break;
                                case "RDS":
                                    break;
                                case "RTP":
                                    break;
                                case "HSD":
                                    break;
                                case "HSS":
                                    break;
                                case "GTO":
                                    break;
                                case "AAD":
                                    break;
                                case "DRGO":
                                    break;
                                case "DCR":
                                    break;
                                case "AAO":
                                    break;
                                case "DDCS":
                                    break;
                                case "ADR":
                                    break;
                                default:
                                    _logger.LogInformation($"Evento {P.Code} recebido para o pedido {P.OrderId}, mas não é tratado no momento.");
                                    break;
                            }
                        }

                    }
                    else
                    {
                        _logger.LogWarning($"Falha ao realizar pooling para o merchant {Merchant.NomeFantasia} erro: {PoolingResponse.StatusCode}");
                        return new ReturnApiRefatored<object>
                        {
                            Status = "error",
                            Messages = new List<string> { $"Falha ao realizar pooling para o merchant {Merchant.NomeFantasia} erro: {PoolingResponse.StatusCode}" }
                        };
                    }
                }

                await Acknowledge(IfoodClient, PollingsToAcknowledge);

            }
            else // CASO NÃO EXISTA NENHUMA EMPRESA IFOOD VINCULADA AO ESTABELECIMENTO
            {
                return new ReturnApiRefatored<object>
                {
                    Status = "error",
                    Messages = new List<string> { "Nenhuma Empresa Ifood Encontrada para esse Estabelecimento!" }
                };

            }


            return new ReturnApiRefatored<object>
            {
                Status = "success",
                Messages = Messages
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Pooling do iFood cancelado. Endpoint: {Pooling}", "Pooling");
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "A requisição para leitura de pedidos demorou muito e foi cancelada." } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar API do iFood. Endpoint: {Pooling}", "Polling");
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao ler pedidos ifood" } };
        }
    }
    #endregion

    #region Funções de Ação dos Pedidos Do Ifood
    private async Task<string> GetPedido(string OrderId, HttpClient IfoodClient)
    {
        var PedidoResponse = await IfoodClient.GetAsync($"order/v1.0/orders/{OrderId}");
        if (PedidoResponse.IsSuccessStatusCode)
        {
            int statusCode = (int)PedidoResponse.StatusCode;


            return await PedidoResponse.Content.ReadAsStringAsync();
        }
        else
        {
            return await PedidoResponse.Content.ReadAsStringAsync() + $"{OrderId}";
        }
    }

    #endregion

    #region Funções de Conversão de Pedido Ifood para Pedido do SOPHOS
    #endregion

    #region Funções Auxiliares
    private void AdicionaTokenNaRequisicao(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task Acknowledge(HttpClient IfoodClient, List<PollingIfoodDto> Pollings)
    {
        await IfoodClient.PostAsJsonAsync($"order/v1.0/events/acknowledgment", Pollings);
    }
    #endregion
}


