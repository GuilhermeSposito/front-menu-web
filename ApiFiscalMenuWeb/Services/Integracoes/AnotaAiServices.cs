using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

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
            case 0:
                retornoDaFuncaoDeAdicionarPedido =  await this.AdicionaPedido(merchant, pedido);
                //await SetPedido(item.IdPedido);                                    //Em análise 
                break;
            case 1:
                retornoDaFuncaoDeAdicionarPedido =  await this.AdicionaPedido(merchant, pedido);
                //await SetPedido(item.IdPedido);                                   //em produção 
                break;
            case 2:
                retornoDaFuncaoDeAdicionarPedido =  await this.AdicionaPedido(merchant, pedido);
                //await SetPedido(item.IdPedido);
                //await AtualizaStatusPedido(item.IdPedido, item.Check);            //pronto
                break;
            case 3:
                //await AtualizaStatusPedido(item.IdPedido, item.Check);            //Finalizado
                break;
            case 4:
                //await AtualizaStatusPedido(item.IdPedido, item.Check);            //Cancelado
                break;
            case 5:
                //await AtualizaStatusPedido(item.IdPedido, item.Check);           //Negado
                break;
            case 6:
                //await AtualizaStatusPedido(item.IdPedido, item.Check);           //Solicitação de cancelamento de pedido
                break;
        }

        Console.WriteLine(retornoDaFuncaoDeAdicionarPedido);

    }

    private async Task<string> AdicionaPedido(ClsMerchant Merchant, AnotaAiOrderInfoDto PedidoAnotaAi)
    {   
        var jsonDoPedido = JsonSerializer.Serialize(PedidoAnotaAi);
        PedidoIfoodDto pedidoIfood = await NormalizaPedidoAnotaAiParaPedidoIfood(PedidoAnotaAi);
        ClsPedido? PedidoSophos = await _ifoodServices.ConvertePedidoDoIfoodParaPedidoSophos(pedidoIfood, Merchant, jsonDoPedido);
        if (PedidoSophos is null)
            throw new BadHttpRequestException("Pedido nullo");

        PedidoSophos.CriadoPor = "ANOTAAI";
        PedidoSophos.IfoodID = null;
        PedidoSophos.IdIntegracao = PedidoAnotaAi.Id;

        bool AdicionouOPedido = await _nestApiService.CriarPedidoSophos(Merchant, PedidoSophos);
        if (!AdicionouOPedido)
            return $"Falha ao adicionar o pedido {PedidoSophos.Id} para o merchant {Merchant.NomeFantasia}";

        if (Merchant.AceitaPedidoAutDeIntegracoes) //Aqui serve para podermos integrar com a loja do cliente mas não aceitar os pedidos pra ele, apenas visualizar 
        {
          
        }

        return $"Pedido {PedidoSophos.Id} {Merchant.NomeFantasia}";
    }

    public async Task<PedidoIfoodDto> NormalizaPedidoAnotaAiParaPedidoIfood(AnotaAiOrderInfoDto pedido)
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
            "money"                     => "CASH",
            "card"                      => "CREDIT",
            "online"                    => "DIGITAL_WALLET",
            "online_credit"             => "CREDIT",
            "pix"                       => "PIX",
            "debit"                     => "DEBIT",
            "pix-ifood"                 => "PIX",
            "online_tuna"               => "DIGITAL_WALLET",
            "ifood-online-credit-payin" => "CREDIT",
            "ifood-online-pix-payin"    => "PIX",
            "tuna_nupay_credit"         => "CREDIT",
            "tuna_nupay_debit"          => "DEBIT",
            "tuna_wallet_credit"        => "DIGITAL_WALLET",
            "tuna_wallet_debit"         => "DIGITAL_WALLET",
            "tuna_minimal_payment_link" => "DIGITAL_WALLET",
            "ifood-pago-credit-pinpad"  => "CREDIT",
            "ifood-pago-debit-pinpad"   => "DEBIT",
            "ifood-pago-pix-pinpad"     => "PIX",
            _                           => "DIGITAL_WALLET"
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
