using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class B1DeliveryServices
{
    #region Propriedades
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiService;
    private readonly IfoodServices _ifoodServices;
    private readonly ILogger<B1DeliveryServices> _logger;
    private readonly IConfiguration _configuration;

    public B1DeliveryServices(
        IHttpClientFactory factory,
        NestApiServices nestApiService,
        IfoodServices ifoodServices,
        ILogger<B1DeliveryServices> logger,
        IConfiguration configuration)
    {
        _factory = factory;
        _nestApiService = nestApiService;
        _ifoodServices = ifoodServices;
        _logger = logger;
        _configuration = configuration;
    }
    #endregion

    #region Polling
    public async Task ExecutarPollingTodasEmpresas()
    {
        try
        {
            var empresas = await _nestApiService.RetornaEmpresasDelmatchParaPolling();
            if (empresas is null || empresas.Count == 0)
                return;

            foreach (var empresa in empresas.Where(e => e.Ativo))
            {
                await PollingDelmatch(empresa);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[B1Delivery] Erro ao executar polling de todas as empresas");
        }
    }

    public async Task PollingDelmatch(DelmatchEmpresaDto empresa)
    {
        try
        {
            var delmatchClient = _factory.CreateClient("ApiDelmatch");
            delmatchClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", empresa.AccessToken);

            var response = await delmatchClient.GetAsync("/api/orders.json");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[B1Delivery] Falha no polling da empresa {Id}. Status: {Status}", empresa.Id, response.StatusCode);
                return;
            }

            var pedidos = await response.Content.ReadFromJsonAsync<List<DelmatchOrderDto>>();
            if (pedidos is null || pedidos.Count == 0)
                return;

            foreach (var pedido in pedidos)
            {
                await AddOrUpdatePedido(pedido, empresa);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[B1Delivery] Erro no polling da empresa {Id}", empresa.Id);
        }
    }

    public async Task AddOrUpdatePedido(DelmatchOrderDto pedido, DelmatchEmpresaDto empresa)
    {
        try
        {
            // Verificar se o pedido já existe no Sophos pelo IfoodID
            var pedidoExistente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(pedido.Id.ToString());

            if (pedidoExistente is null)
            {
                await AdicionaPedidoAoSophos(pedido, empresa);
            }
            // Se já existe — não faz nada no polling, mudanças de status vêm pelo webhook
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[B1Delivery] Erro ao processar pedido {PedidoId}", pedido.Id);
        }
    }
    #endregion

    #region Conversão Delmatch → Sophos
    public PedidoIfoodDto NormalizaDelmatchParaFormatoIfood(DelmatchOrderDto pedido)
    {
        // Calcular descontos e acréscimos totais
        decimal totalDesconto = pedido.Items.Sum(i => i.Discount + i.SubItems.Sum(s => s.Discount));
        decimal totalAcrescimo = pedido.Items.Sum(i => i.Addition + i.SubItems.Sum(s => s.Addition));

        var pedidoIfood = new PedidoIfoodDto
        {
            Id = pedido.Id.ToString(),
            DisplayId = pedido.ShortReference.ToString(),
            OrderType = pedido.Type == "DELIVERY" ? "DELIVERY" : "TAKEOUT",
            OrderTiming = pedido.ScheduleDateTime != null ? "SCHEDULED" : "IMEDIATE",
            CreatedAt = DateTime.TryParse(pedido.CreatedAt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dtCreated) ? dtCreated : DateTime.UtcNow,
            ExtraInfo = string.Empty,

            Total = new TotalIfoodDto
            {
                SubTotal = (double)pedido.SubTotal,
                OrderAmount = (double)pedido.TotalPrice,
                DeliveryFee = (double)pedido.DeliveryFee,
                AddtionalFees = (double)totalAcrescimo,
            },

            Customer = new CustomerIfoodDto
            {
                Id = ApenasDigitos(pedido.Customer.Phone),
                Name = pedido.Customer.Name,
                Phone = new PhoneIfoodDto { Localizer = ApenasDigitos(pedido.Customer.Phone) }
            },

            Delivery = new DeliveryIfoodDto
            {
                DeliveryAddress = new DeliveryAddresIfoodDto
                {
                    StreetName = pedido.DeliveryAddress.StreetName,
                    StreetNumber = pedido.DeliveryAddress.StreetNumber,
                    Neighborhood = pedido.DeliveryAddress.Neighbourhood ?? string.Empty,
                    Complement = pedido.DeliveryAddress.Complement ?? string.Empty,
                    Reference = pedido.DeliveryAddress.Reference ?? string.Empty,
                    PostalCode = pedido.DeliveryAddress.PostalCode,
                    City = pedido.DeliveryAddress.City,
                    State = pedido.DeliveryAddress.State,
                    FormattedAddress = pedido.DeliveryAddress.FormattedAddress,
                }
            },
        };

        // Agendamento
        if (pedido.ScheduleDateTime != null &&
            DateTime.TryParse(pedido.ScheduleDateTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dtSchedule))
        {
            if (pedidoIfood.OrderType == "DELIVERY")
                pedidoIfood.Delivery.DeliveryDateTime = dtSchedule;
        }

        // Benefits (descontos)
        if (totalDesconto > 0)
        {
            pedidoIfood.Benefits.Add(new BenefitsIfoodDto
            {
                Value = (double)totalDesconto,
                Description = new List<SponsorShipValuesIfoodDto>
                {
                    new SponsorShipValuesIfoodDto { Name = "DELMATCH", Value = (double)totalDesconto }
                }
            });
        }

        // Itens
        foreach (var item in pedido.Items)
        {
            var itemIfood = new ItemIfoodDto
            {
                Name = item.Name,
                Quantity = (double)item.Quantity,
                UnitPrice = (double)item.Price,
                TotalPrice = (double)item.TotalPrice,
                ExternalCode = item.ExternalCode,
                Observations = item.Observations ?? string.Empty,
            };

            foreach (var sub in item.SubItems)
            {
                itemIfood.Options.Add(new OptionsIfoodDto
                {
                    Name = sub.Name,
                    Quantity = (double)sub.Quantity,
                    UnitPrice = (double)sub.Price,
                    Price = (double)sub.TotalPrice,
                    ExternalCode = sub.ExternalCode,
                });
            }

            pedidoIfood.Items.Add(itemIfood);
        }

        // Pagamentos
        pedidoIfood.Payments = new PaymentIfoodDto();
        foreach (DelmatchPaymentDto pag in pedido.Payments)
        {
            var pagConvertido = new MethodsIfoodDto
            {
                Type = pag.Prepaid ? "ONLINE" : "OFFLINE",
                Method = MapeiaNomeParaCodeIfood(pag.Name),
                Value = (double)pag.Value,
            };

            if (pag.Code == "DIN" && pag.CashChange > 0)
            {
                // Convencao iFood: Value = total do pedido, ChangeFor = valor pago pelo cliente
                // Assim: Troco = ChangeFor - Value (calculado em IfoodServices)
                pagConvertido.Value = (double)pedido.TotalPrice;
                pagConvertido.Cash = new CashMethodsIfoodDto
                {
                    ChangeFor = (double)pag.Value
                };
            }

            pedidoIfood.Payments.Methods.Add(pagConvertido);
        }

        return pedidoIfood;
    }

    public async Task<ClsPedido?> ConvertePedidoDelmatchParaPedidoSophos(DelmatchOrderDto pedido, ClsMerchant merchant)
    {
        var pedidoIfood = NormalizaDelmatchParaFormatoIfood(pedido);
        var pedidoSophos = await _ifoodServices.ConvertePedidoDoIfoodParaPedidoSophos(pedidoIfood, merchant);

        if (pedidoSophos is null)
            return null;

        pedidoSophos.CriadoPor = "DELMATCH";
        pedidoSophos.JsonPedidoDeIntegracao = JsonSerializer.Serialize(pedido);
        // Delmatch IDs são numéricos — não são UUIDs. Usar IdIntegracao (text) em vez de IfoodID (uuid)
        pedidoSophos.IdIntegracao = pedidoSophos.IfoodID;
        pedidoSophos.IfoodID = null;

        // Mesmo padrão do iFood: se o merchant não aceita pedidos automaticamente,
        // o pedido entra como NOVO/ABERTO para aceite manual no painel
        if (!merchant.AceitaPedidoAutDeIntegracoes)
        {
            pedidoSophos.EtapaPedido = "NOVO";
            pedidoSophos.StatusPedido = "ABERTO";
        }

        return pedidoSophos;
    }

    private string MapeiaNomeParaCodeIfood(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) return "CREDIT";

        var n = nome.Trim().ToUpperInvariant()
                    .Replace("É", "E").Replace("Ê", "E")
                    .Replace("Ã", "A").Replace("Á", "A").Replace("Â", "A")
                    .Replace("Ç", "C").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");

        if (n.Contains("CREDITO") || n.Contains("CREDIT")) return "CREDIT";
        if (n.Contains("DEBITO") || n.Contains("DEBIT") || n == "DEB") return "DEBIT";
        if (n.Contains("DINHEIRO") || n.Contains("MONEY") || n == "Dinheiro" || n == "CASH") return "CASH";
        if (n.Contains("PIX")) return "PIX";
        if (n.Contains("REFEI")) return "MEAL_VOUCHER";
        if (n.Contains("ALIMENT")) return "FOOD_VOUCHER";
        if (n.Contains("OUTROS") || n.Contains("OTHER")) return "CASH";

        return "CREDIT"; // fallback
    }

    private static string ApenasDigitos(string valor) =>
        string.IsNullOrEmpty(valor) ? string.Empty : Regex.Replace(valor, @"\D", "");
    #endregion

    #region Ações de Status no Delmatch
    public async Task<bool> AceitaPedidoDelmatch(int pedidoId, string accessToken)
        => await EnviaStatusDelmatch(pedidoId, "confirmation", accessToken);

    public async Task<bool> CancelaPedidoDelmatch(int pedidoId, string accessToken)
        => await EnviaStatusDelmatch(pedidoId, "cancellation", accessToken);

    public async Task<bool> DespacharPedidoDelmatch(int pedidoId, string accessToken)
        => await EnviaStatusDelmatch(pedidoId, "dispatch", accessToken);

    public async Task<bool> ConcluirPedidoDelmatch(int pedidoId, string accessToken)
        => await EnviaStatusDelmatch(pedidoId, "delivered", accessToken);

    private async Task<bool> EnviaStatusDelmatch(int pedidoId, string acao, string accessToken)
    {
        try
        {
            var client = _factory.CreateClient("ApiDelmatch");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var body = new StringContent("{\"success\":true}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/orders/{pedidoId}/statuses/{acao}.json", body);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("[B1Delivery] Falha ao enviar status '{Acao}' para pedido {PedidoId}. HTTP {Status}", acao, pedidoId, response.StatusCode);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[B1Delivery] Erro ao enviar status '{Acao}' para pedido {PedidoId}", acao, pedidoId);
            return false;
        }
    }
    #endregion

    #region Adicionar Pedido ao Sophos
    public async Task AdicionaPedidoAoSophos(DelmatchOrderDto pedido, DelmatchEmpresaDto empresa)
    {
        if (empresa.MerchantSophos is null)
        {
            _logger.LogWarning("[B1Delivery] Merchant Sophos não encontrado para empresa {Id}", empresa.Id);
            return;
        }

        ClsPedido? pedidoSophos = await ConvertePedidoDelmatchParaPedidoSophos(pedido, empresa.MerchantSophos);
        if (pedidoSophos is null)
        {
            _logger.LogWarning("[B1Delivery] Falha ao converter pedido {PedidoId}", pedido.Id);
            return;
        }

        if (empresa.MerchantSophos.AceitaPedidoAutDeIntegracoes)
        { 
            pedidoSophos.EtapaPedido = "PREPARANDO";
        }

        bool adicionou = await _nestApiService.CriarPedidoSophos(empresa.MerchantSophos, pedidoSophos);
        if (!adicionou)
        {
            _logger.LogWarning("[B1Delivery] Falha ao salvar pedido {PedidoId} no Sophos. Payload enviado: {Payload}",
                pedido.Id,
                JsonSerializer.Serialize(pedidoSophos));
            return;
        }

        if (empresa.MerchantSophos.AceitaPedidoAutDeIntegracoes)
        {
            await AceitaPedidoDelmatch(pedido.Id, empresa.AccessToken);
        }
    }

    /// <summary>
    /// Aceite manual de pedido Delmatch — chamado pelo painel quando o merchant aceita um pedido NOVO.
    /// Confirma no Delmatch e atualiza para PREPARANDO no Sophos.
    /// </summary>
    public async Task<bool> AceitarPedidoManual(string idIntegracao, string merchantSophosId)
    {
        // Buscar o pedido no Sophos pelo IdIntegracao
        var pedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(idIntegracao);
        if (pedidoSophos is null)
        {
            _logger.LogWarning("[B1Delivery] AceitarPedidoManual: pedido {Id} não encontrado no Sophos", idIntegracao);
            return false;
        }

        // Buscar a empresa Delmatch pelo merchantSophosId para obter o AccessToken
        var empresas = await _nestApiService.RetornaEmpresasDelmatchParaPolling();
        var empresa = empresas.FirstOrDefault(e => e.MerchantSophos?.Id == merchantSophosId);
        if (empresa is null)
        {
            _logger.LogWarning("[B1Delivery] AceitarPedidoManual: empresa Delmatch não encontrada para merchant {MerchantId}", merchantSophosId);
            return false;
        }

        // Confirmar no Delmatch
        if (!int.TryParse(idIntegracao, out var pedidoIdDelmatch))
        {
            _logger.LogWarning("[B1Delivery] AceitarPedidoManual: idIntegracao '{Id}' não é numérico", idIntegracao);
            return false;
        }

        await AceitaPedidoDelmatch(pedidoIdDelmatch, empresa.AccessToken);

        // Atualizar para PREPARANDO no Sophos
        await _nestApiService.UpdatePedidoPreparandoNaAPiPrincipalAsync(
            null, merchantSophosId, pedidoSophos);

        return true;
    }
    #endregion

    #region Processar Webhook
    public async Task ProcessarWebhookAsync(DelmatchWebhookDto webhook)
    {
        try
        {
            var empresa = await _nestApiService.RetornaEmpresaDelmatchPeloId(webhook.EmpresaId);
            if (empresa is null || !empresa.Ativo)
            {
                _logger.LogInformation("[B1Delivery] Empresa {EmpresaId} não encontrada ou inativa — webhook ignorado.", webhook.EmpresaId);
                return;
            }

            switch (webhook.Status)
            {
                case "Aguardando aceite":
                    var delmatchClient = _factory.CreateClient("ApiDelmatch");
                    delmatchClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", empresa.AccessToken);

                    var ordersResponse = await delmatchClient.GetAsync("/api/orders.json");
                    if (!ordersResponse.IsSuccessStatusCode)
                        return;

                    var ordersJson = await ordersResponse.Content.ReadAsStringAsync();
                    var delmatchJsonOptions = new JsonSerializerOptions
                    {
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                        PropertyNameCaseInsensitive = true,
                    };
                    List<DelmatchOrderDto>? pedidos;
                    try
                    {
                        pedidos = JsonSerializer.Deserialize<List<DelmatchOrderDto>>(ordersJson, delmatchJsonOptions);
                    }
                    catch (JsonException jex)
                    {
                        _logger.LogError(jex, "[B1Delivery] Falha ao deserializar pedidos. JSON recebido: {Json}", ordersJson);
                        return;
                    }

                    var pedidoNovo = pedidos?.FirstOrDefault(p => p.Id == webhook.Id);
                    if (pedidoNovo is not null)
                        await AdicionaPedidoAoSophos(pedidoNovo, empresa);
                    break;

                case "Recebido":
                    {
                        var pedidoExistente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(webhook.Id.ToString());
                        if (pedidoExistente is not null)
                            await _nestApiService.UpdatePedidoPreparandoNaAPiPrincipalAsync(
                                null, empresa.MerchantSophos?.Id ?? string.Empty, pedidoExistente);
                        else
                            _logger.LogWarning("[B1Delivery] Pedido {Id} não encontrado para atualizar status Recebido", webhook.Id);
                        break;
                    }

                case "Despachado":
                    {
                        var pedidoExistente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(webhook.Id.ToString());
                        if (pedidoExistente is not null)
                            await _nestApiService.UpdatePedidoDespachadoNaAPiPrincipalAsync(
                                null, empresa.MerchantSophos?.Id, pedidoExistente);
                        else
                            _logger.LogWarning("[B1Delivery] Pedido {Id} não encontrado para atualizar status Despachado", webhook.Id);
                        break;
                    }

                case "Entregue":
                    {
                        var pedidoExistente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(webhook.Id.ToString());
                        if (pedidoExistente is not null)
                            await _nestApiService.UpdatePedidoConcluidodoNaAPiPrincipalAsync(
                                null, empresa.MerchantSophos?.Id, pedidoExistente);
                        else
                            _logger.LogWarning("[B1Delivery] Pedido {Id} não encontrado para atualizar status Entregue", webhook.Id);
                        break;
                    }

                case "Cancelado":
                    {
                        var pedidoExistente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(webhook.Id.ToString());
                        if (pedidoExistente is not null)
                            await _nestApiService.UpdatePedidoCanceladodoNaAPiPrincipalAsync(pedidoExistente);
                        else
                            _logger.LogWarning("[B1Delivery] Pedido {Id} não encontrado para atualizar status Cancelado", webhook.Id);
                        break;
                    }

                default:
                    _logger.LogInformation("[B1Delivery] Status desconhecido '{Status}' para pedido {Id}", webhook.Status, webhook.Id);
                    break;
            }
        }
        catch (JsonException jex)
        {
            _logger.LogError(jex, "[B1Delivery] Erro de conversão JSON ao processar webhook do pedido {Id}", webhook.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[B1Delivery] Erro ao processar webhook do pedido {Id}", webhook.Id);
        }
    }
    #endregion
}
