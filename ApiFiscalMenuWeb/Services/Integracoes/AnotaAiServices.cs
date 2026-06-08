using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using System.Net.Http.Headers;
using System.Text.Json;
using ApiFiscalMenuWeb.Models;
using FrontMenuWeb.DTOS;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class AnotaAiServices
{
    #region Propriedades
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiService;
    private readonly IfoodServices _ifoodServices;
    private readonly ILogger<AnotaAiServices> _logger;
    private readonly IConfiguration _configuration;

    public AnotaAiServices(
        IHttpClientFactory factory,
        NestApiServices nestApiService,
        IfoodServices ifoodServices,
        ILogger<AnotaAiServices> logger,
        IConfiguration configuration)
    {
        _factory = factory;
        _nestApiService = nestApiService;
        _ifoodServices = ifoodServices;
        _logger = logger;
        _configuration = configuration;
    }
    #endregion

    #region Funções de SetPedido
    public async Task TrataPedidoAnotaAi(string MerchantId, AnotaAiOrderInfoDto pedido)
    {
        ClsMerchant? merchant = await _nestApiService.GetMerchantFromNestApiPublic(MerchantId);
        if (merchant == null)
        {
            throw new BadHttpRequestException("Merchant não encontrado");
        }

        string? retornoDaFuncaoDeAdicionarPedido = string.Empty;
        switch (pedido.Check)
        {
            case -2:
                //Pedido agendado aceito
                break;
            case 0: //novo pedido
                retornoDaFuncaoDeAdicionarPedido = await this.AdicionaPedido(merchant, pedido);
                break;
            case 1: //em produção 
                await AtualizaStatusPedido(pedido.Id, merchant, pedido.Check);
                // retornoDaFuncaoDeAdicionarPedido = await this.AdicionaPedido(merchant, pedido);
                break;
            case 2:  //pronto
                     // retornoDaFuncaoDeAdicionarPedido = await this.AdicionaPedido(merchant, pedido);
                await AtualizaStatusPedido(pedido.Id, merchant, pedido.Check);
                break;
            case 3:  //Finalizado
                await AtualizaStatusPedido(pedido.Id, merchant, pedido.Check);
                break;
            case 4:   //Cancelado
                await AtualizaStatusPedido(pedido.Id, merchant, pedido.Check);
                break;
            case 5:   //Negado
                await AtualizaStatusPedido(pedido.Id, merchant, pedido.Check);
                break;
            case 6:   //Solicitação de cancelamento de pedido
                await AtualizaStatusPedido(pedido.Id, merchant, pedido.Check);
                break;
        }

        Console.WriteLine(retornoDaFuncaoDeAdicionarPedido);
    }

    private async Task AtualizaStatusPedido(string integracaoId, ClsMerchant merchant, int check)
    {
        var pedido = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(integracaoId);
        if (pedido is null)
        {
            _logger.LogWarning("[AnotaAi] Pedido com integracaoId {Id} não encontrado para atualizar status (check={Check})", integracaoId, check);
            return;
        }

        switch (check)
        {
            case 1: // em produção — aceita o pedido
                await _nestApiService.UpdatePedidoPreparandoNaAPiPrincipalAsync(null, merchant.Id, pedido);
                break;
            case 2: // pronto — despacha o pedido
                await _nestApiService.UpdatePedidoDespachadoNaAPiPrincipalAsync(null, merchant.Id, pedido);
                break;
            case 3: // finalizado
                await _nestApiService.UpdatePedidoConcluidodoNaAPiPrincipalAsync(null, merchant.Id, pedido);
                break;
            case 4: // cancelado
            case 5: // negado
                await _nestApiService.UpdatePedidoCanceladodoNaAPiPrincipalAsync(pedido);
                break;
            case 6: // solicitação de cancelamento
                await _nestApiService.UpdatePedidoInfosAdicionaisOuStatusoNaAPiPrincipalAsync(merchant.Id, pedido, new UpdatePedidoInfosAdicionaisDto { InfoAdicionalOuStatus = "CANCELAMENTO_SOLICITADO" });
                break;
        }
    }

    private async Task<string> AdicionaPedido(ClsMerchant Merchant, AnotaAiOrderInfoDto PedidoAnotaAi)
    {
        var jsonDoPedido = JsonSerializer.Serialize(PedidoAnotaAi);
        PedidoIfoodDto pedidoIfood = NormalizaPedidoAnotaAiParaPedidoIfood(PedidoAnotaAi);
        ClsPedido? PedidoSophos = await _ifoodServices.ConvertePedidoDoIfoodParaPedidoSophos(pedidoIfood, Merchant, jsonDoPedido);
        if (PedidoSophos is null)
            return $"Falha ao adicionar o pedido {PedidoAnotaAi.Id} para o merchant {Merchant.NomeFantasia}";

        PedidoSophos.CriadoPor = "ANOTAAI";
        PedidoSophos.IfoodID = null;
        PedidoSophos.IdIntegracao = PedidoAnotaAi.Id;


        bool AdicionouOPedido = await _nestApiService.CriarPedidoSophos(Merchant, PedidoSophos);
        if (!AdicionouOPedido)
            return $"Falha ao adicionar o pedido {PedidoAnotaAi.Id} para o merchant {Merchant.NomeFantasia}";

        if (Merchant.AceitaPedidoAutDeIntegracoes && !string.IsNullOrEmpty(Merchant.TokenAnotaAi))
            await AceitarPedidoAnotaAiAsync(PedidoAnotaAi.Id, Merchant.TokenAnotaAi);

        return $"Pedido {PedidoSophos.Id} {Merchant.NomeFantasia}";
    }

    private async Task<bool> AceitarPedidoAnotaAiAsync(string orderId, string tokenAnotaAi)
    {
        try
        {
            var client = _factory.CreateClient("ApiAnotaAi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAnotaAi);

            var response = await client.PostAsync($"order/accept/{orderId}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[AnotaAi] Falha ao aceitar pedido {OrderId}. Status: {Status}", orderId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("[AnotaAi] Pedido {OrderId} aceito com sucesso.", orderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AnotaAi] Erro ao aceitar pedido {OrderId}", orderId);
            return false;
        }
    }

    public async Task<bool> AceitarPedidoManualAsync(string idIntegracao, string merchantSophosId)
    {
        var merchant = await _nestApiService.GetMerchantFromNestApiPublic(merchantSophosId);
        if (merchant is null || string.IsNullOrEmpty(merchant.TokenAnotaAi))
        {
            _logger.LogWarning("[AnotaAi] Merchant {MerchantId} não encontrado ou sem TokenAnotaAi.", merchantSophosId);
            return false;
        }
        return await AceitarPedidoAnotaAiAsync(idIntegracao, merchant.TokenAnotaAi);
    }

    // Usa o token do usuário logado (endpoint protegido, retorna TokenAnotaAi) quando disponível;
    // caso contrário, cai no endpoint público pelo MerchantId.
    private async Task<ClsMerchant?> ObterMerchantComTokenAsync(string? tokenNest, string? merchantId)
    {
        if (!string.IsNullOrEmpty(tokenNest))
            return await _nestApiService.GetMerchantFromNestApi(tokenNest);

        if (!string.IsNullOrEmpty(merchantId))
            return await _nestApiService.GetMerchantFromNestApiPublic(merchantId);

        return null;
    }

    public async Task<bool> FinalizarPedidoAsync(UpdatePedidosDto dto)
    {
        if (string.IsNullOrEmpty(dto.PedidoIdIntegracao))
            return false;

        var merchant = await ObterMerchantComTokenAsync(dto.TokenNestApi, dto.MerchantId);
        if (merchant is null || string.IsNullOrEmpty(merchant.TokenAnotaAi))
        {
            _logger.LogWarning("[AnotaAi] Merchant não encontrado ou sem TokenAnotaAi ao finalizar pedido {OrderId}.", dto.PedidoIdIntegracao);
            return false;
        }

        try
        {
            var client = _factory.CreateClient("ApiAnotaAi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", merchant.TokenAnotaAi);

            var response = await client.PostAsync($"order/finalize/{dto.PedidoIdIntegracao}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[AnotaAi] Falha ao finalizar pedido {OrderId}. Status: {Status}", dto.PedidoIdIntegracao, response.StatusCode);
                return false;
            }

            _logger.LogInformation("[AnotaAi] Pedido {OrderId} finalizado.", dto.PedidoIdIntegracao);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AnotaAi] Erro ao finalizar pedido {OrderId}", dto.PedidoIdIntegracao);
            return false;
        }
    }

    public async Task<bool> AvisaPedidoProntoAsync(UpdatePedidosDto dto)
    {
        if (string.IsNullOrEmpty(dto.PedidoIdIntegracao))
            return false;

        var merchant = await ObterMerchantComTokenAsync(dto.TokenNestApi, dto.MerchantId);
        if (merchant is null || string.IsNullOrEmpty(merchant.TokenAnotaAi))
        {
            _logger.LogWarning("[AnotaAi] Merchant não encontrado ou sem TokenAnotaAi ao avisar pronto {OrderId}.", dto.PedidoIdIntegracao);
            return false;
        }

        try
        {
            var client = _factory.CreateClient("ApiAnotaAi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", merchant.TokenAnotaAi);

            var response = await client.PostAsync($"order/ready/{dto.PedidoIdIntegracao}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[AnotaAi] Falha ao avisar pedido pronto {OrderId}. Status: {Status}", dto.PedidoIdIntegracao, response.StatusCode);
                return false;
            }

            _logger.LogInformation("[AnotaAi] Pedido {OrderId} marcado como pronto.", dto.PedidoIdIntegracao);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AnotaAi] Erro ao avisar pedido pronto {OrderId}", dto.PedidoIdIntegracao);
            return false;
        }
    }

    public PedidoIfoodDto NormalizaPedidoAnotaAiParaPedidoIfood(AnotaAiOrderInfoDto pedido)
    {
        var PedidoIfood = new PedidoIfoodDto();
        PedidoIfood.Id = pedido.Id;
        PedidoIfood.DisplayId = pedido.IdAlt;
        PedidoIfood.ExtraInfo = pedido.Observation ?? String.Empty;
        PedidoIfood.OrderTiming = "IMMEDIATE";
        pedido.Type = pedido.Type switch
        {
            "LOCAL" => "INDOOR",
            "DELIVERY" => "DELIVERY",
            "TAKE" => "TAKEOUT",
            _ => pedido.Type
        };
        PedidoIfood.CreatedAt = pedido.CreatedAt;
        PedidoIfood.Customer = new CustomerIfoodDto
        {
            Name = pedido.Customer?.Name ?? "",
            Phone = new PhoneIfoodDto
            {
                Number = pedido.Customer?.Phone ?? "",
                Localizer = pedido.Customer?.Phone ?? "",
            },
            DocumentNumber = pedido.Customer?.TaxPayerIdentificationNumber ?? "",
        };
        PedidoIfood.Delivery = new DeliveryIfoodDto
        {
            DeliveryAddress = new DeliveryAddresIfoodDto
            {
                StreetName = pedido.DeliveryAddress?.StreetName ?? "",
                StreetNumber = pedido.DeliveryAddress?.StreetNumber ?? "",
                Complement = pedido.DeliveryAddress?.Complement ?? "",
                City = pedido.DeliveryAddress?.City ?? "",
                State = pedido.DeliveryAddress?.State ?? "",
                FormattedAddress = pedido.DeliveryAddress?.FormattedAddress ?? "",
                Country = pedido.DeliveryAddress?.Country ?? "",
                Neighborhood = pedido.DeliveryAddress?.Neighborhood ?? "",
                PostalCode = pedido.DeliveryAddress?.PostalCode ?? "",
                Coordinates = new CoordinatesIfoodDto
                {
                    Latitude = pedido.DeliveryAddress?.Coordinates?.Latitude ?? 0,
                    Longitude = pedido.DeliveryAddress?.Coordinates?.Longitude ?? 0
                }
            }
        };
        PedidoIfood.Items = pedido.Items.Select(x => new ItemIfoodDto
        {
            Id = x.Id,
            Name = x.Name,
            Quantity = x.Quantity,
            UnitPrice = x.Price,
            TotalPrice = x.Total,
            ExternalCode = x.ExternalId,
            Options = x.SubItems.Select(s => new OptionsIfoodDto
            {
                Name = s.Name,
                Quantity = s.Quantity,
                UnitPrice = s.Price,
                Price = s.Total,
                ExternalCode = s.ExternalCode
            }).ToList()
        }).ToList();
        PedidoIfood.Total = new TotalIfoodDto
        {
            OrderAmount = (double)pedido.Total,
            DeliveryFee = (double)pedido.DeliveryFee,
            SubTotal = (double)pedido.Items.Sum(x => x.Total),
            Benefits = (double)pedido.Discounts.Sum(x => x.Amount),
            AddtionalFees = (double)pedido.AdditionalFees.Sum(x => x.Value)
        };
        PedidoIfood.Benefits = pedido.Discounts.Select(x => new BenefitsIfoodDto
        {
            Description = new List<SponsorShipValuesIfoodDto>
            {
                new SponsorShipValuesIfoodDto
                {
                    Description = "MERCHANT",
                    Name = x.Tag,
                    Value = (double)x.Amount,
                }
            },

        }).ToList();

        PedidoIfood.Payments = new PaymentIfoodDto
        {
            Methods = pedido.Payments.Select(p => new MethodsIfoodDto
            {
                Method = RetornaTipoDePagamentoIgualDoIfood(p.Code),
                Type = p.Prepaid ? "ONLINE" : "OFFLINE",
                Value = (double)p.Value,
                Cash = new CashMethodsIfoodDto
                {
                    ChangeFor = (double)(p.ChangeFor ?? 0)
                }
            }).ToList()
        };

        return PedidoIfood;
    }

    private string RetornaTipoDePagamentoIgualDoIfood(string code)
    {
        return code?.Trim().ToLowerInvariant() switch
        {
            "money" => "CASH",
            "card" => "CREDIT",
            "online" => "DIGITAL_WALLET",
            "online_credit" => "CREDIT",
            "pix" => "PIX",
            "debit" => "DEBIT",
            "pix-ifood" => "PIX",
            "online_tuna" => "DIGITAL_WALLET",
            "ifood-online-credit-payin" => "CREDIT",
            "ifood-online-pix-payin" => "PIX",
            "tuna_nupay_credit" => "CREDIT",
            "tuna_nupay_debit" => "DEBIT",
            "tuna_wallet_credit" => "DIGITAL_WALLET",
            "tuna_wallet_debit" => "DIGITAL_WALLET",
            "tuna_minimal_payment_link" => "DIGITAL_WALLET",
            "ifood-pago-credit-pinpad" => "CREDIT",
            "ifood-pago-debit-pinpad" => "DEBIT",
            "ifood-pago-pix-pinpad" => "PIX",
            _ => "DIGITAL_WALLET"
        };
    }

    public async Task GetPedidoAnotaAi(string MerchantId, string PedidoId)
    {
        ClsMerchant? merchant = await _nestApiService.GetMerchantFromNestApi(MerchantId);
        if (merchant == null)
        {
            throw new BadHttpRequestException("Merchant não encontrado");
        }


    }
    #endregion
}
