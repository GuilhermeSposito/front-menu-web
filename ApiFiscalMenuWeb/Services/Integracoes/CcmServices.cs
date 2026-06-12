using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Models.Dtos.Ccm;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class CcmServices
{
    #region Propriedades
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiService;
    private readonly IfoodServices _ifoodServices;
    private readonly ILogger<CcmServices> _logger;
    private readonly IConfiguration _configuration;

    public CcmServices(
        IHttpClientFactory factory,
        NestApiServices nestApiService,
        IfoodServices ifoodServices,
        ILogger<CcmServices> logger,
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
    public async Task PollingInit()
    {
        try
        {
            var merchants = await _nestApiService.RetornaMerchantsComIntegracaoCcm();
            if (merchants is null || merchants.Count == 0)
                return;

            foreach (var merchant in merchants)
            {
                if (string.IsNullOrEmpty(merchant.TokenCcm))
                    continue;

                // Fire-and-forget: não aguardamos para não travar a thread do polling
                _ = PollingParaCadaEmpresaCCM(merchant);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao executar o polling de todas as empresas CCM");
        }
    }

    public async Task PollingParaCadaEmpresaCCM(ClsMerchant merchant)
    {
        try
        {
            // Avisa o CCM que o integrador está ativo antes de buscar os pedidos
            await EnviaActivePingCcmAsync(merchant);

            var client = _factory.CreateClient("ApiCcm");
            var response = await client.GetAsync($"wsccm_v2.php?token={merchant.TokenCcm}&codFilial={merchant.IdTokenCcm}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CCM] Falha no polling do merchant {MerchantId}. Status: {Status}", merchant.Id, response.StatusCode);
                return;
            }

            var xml = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(xml))
                return;

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CCM] Falha ao interpretar o XML do merchant {MerchantId}. Retorno: {Xml}", merchant.Id, xml);
                return;
            }

            var serializer = new XmlSerializer(typeof(Pedido));
            foreach (var nodePedido in doc.Root?.Elements("pedido") ?? Enumerable.Empty<XElement>())
            {
                Pedido? pedido;
                try
                {
                    using var reader = nodePedido.CreateReader();
                    pedido = serializer.Deserialize(reader) as Pedido;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CCM] Falha ao desserializar pedido do merchant {MerchantId}. Xml: {Xml}", merchant.Id, nodePedido.ToString());
                    continue;
                }

                if (pedido is null)
                    continue;

                // Evita duplicar: só adiciona se o pedido ainda não existe no Sophos
                var pedidoExistente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(pedido.NroPedido.ToString());
                if (pedidoExistente is not null)
                    continue;

                await AdicionarPedidoAoSophos(pedido, merchant, nodePedido.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro no polling do merchant {MerchantId}", merchant.Id);
        }
    }
    #endregion

    #region Adicionar Pedido ao Sophos
    public async Task AdicionarPedidoAoSophos(Pedido pedidoCcm, ClsMerchant merchant, string xmlCru)
    {
        try
        {
            PedidoIfoodDto pedidoIfood = NormalizaPedidoCcmEmPedidoIfood(pedidoCcm);

            // O xml cru é passado como "jsonDoPedido" para ser salvo no campo JsonPedidoDeIntegracao do pedido Sophos
            ClsPedido? pedidoSophos = await _ifoodServices.ConvertePedidoDoIfoodParaPedidoSophos(pedidoIfood, merchant, xmlCru);
            if (pedidoSophos is null)
            {
                _logger.LogWarning("[CCM] Falha ao converter o pedido {NroPedido} do merchant {MerchantId}", pedidoCcm.NroPedido, merchant.Id);
                return;
            }

            pedidoSophos.CriadoPor = "CCM";
            pedidoSophos.IdIntegracao = pedidoCcm.NroPedido.ToString();
            pedidoSophos.IfoodID = null;

            bool adicionou = await _nestApiService.CriarPedidoSophos(merchant, pedidoSophos);
            if (!adicionou)
            {
                _logger.LogWarning("[CCM] Falha ao salvar o pedido {NroPedido} no Sophos para o merchant {MerchantId}", pedidoCcm.NroPedido, merchant.Id);
                return;
            }

            // Aceite automático quando o merchant está configurado para isso
            if (merchant.AceitaPedidoAutDeIntegracoes && !string.IsNullOrEmpty(merchant.TokenCcm))
                await AceitarPedidoCcmAsync(pedidoCcm.NroPedido, merchant.TokenCcm, pedidoCcm.Cliente?.Codigo ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao adicionar o pedido {NroPedido} ao Sophos", pedidoCcm.NroPedido);
        }
    }
    #endregion

    #region Ações no CCM
    private const string MensagemDeAceite = "Seu pedido foi aceito e está em preparo, para mais informações contate-nos";
    private const string MensagemDeRecusa = "Pedido Recusado Pela Loja, entre em contato para mais informações.";

    public async Task<bool> AceitarPedidoCcmAsync(int nroPedido, string tokenCcm, int codCliente = 0, string? mensagem = null)
    {
        try
        {
            mensagem ??= MensagemDeAceite;

            var client = _factory.CreateClient("ApiCcm");
            var msg = Uri.EscapeDataString(mensagem);
            var response = await client.PostAsync($"wsccm.php?token={tokenCcm}&funcao=aceitarPedido&pedido={nroPedido}&msg={msg}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CCM] Falha ao aceitar o pedido {NroPedido}. Status: {Status}", nroPedido, response.StatusCode);
                return false;
            }

            _logger.LogInformation("[CCM] Pedido {NroPedido} aceito com sucesso.", nroPedido);

            // Sempre depois do aceite, limpa o pedido para não vir nos próximos pollings
            await LimparPedidoDoPollingCcmAsync(nroPedido, tokenCcm);

            // Notifica o cliente via push do CCM
            if (codCliente > 0)
                await EnviaPushParaClienteCcmAsync(codCliente, tokenCcm, mensagem);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao aceitar o pedido {NroPedido}", nroPedido);
            return false;
        }
    }

    public async Task<bool> RecusarPedidoCcmAsync(int nroPedido, string tokenCcm)
    {
        try
        {
            var client = _factory.CreateClient("ApiCcm");
            var msg = Uri.EscapeDataString(MensagemDeRecusa);
            var response = await client.PostAsync($"wsccm.php?token={tokenCcm}&funcao=recusarPedido&pedido={nroPedido}&msg={msg}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CCM] Falha ao recusar o pedido {NroPedido}. Status: {Status}", nroPedido, response.StatusCode);
                return false;
            }

            _logger.LogInformation("[CCM] Pedido {NroPedido} recusado.", nroPedido);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao recusar o pedido {NroPedido}", nroPedido);
            return false;
        }
    }

    /// <summary>
    /// Atualiza o status do pedido no acompanhamento do CCM. Valores: 5 = despachado, 6 = concluído.
    /// </summary>
    public async Task<bool> AtualizaStatusPedidoCcmAsync(int nroPedido, string tokenCcm, int valor)
    {
        try
        {
            var client = _factory.CreateClient("ApiCcm");
            var response = await client.PostAsync($"wsccm.php?token={tokenCcm}&funcao=updateStatus&pedido={nroPedido}&valor={valor}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CCM] Falha ao atualizar status do pedido {NroPedido} para {Valor}. Status: {Status}", nroPedido, valor, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao atualizar status do pedido {NroPedido} para {Valor}", nroPedido, valor);
            return false;
        }
    }

    public async Task EnviaPushParaClienteCcmAsync(int codCliente, string tokenCcm, string mensagem)
    {
        try
        {
            var client = _factory.CreateClient("ApiCcm");
            var msg = Uri.EscapeDataString(mensagem);
            var response = await client.PostAsync($"wsccm_v2.php?token={tokenCcm}&funcao=pushCliente&msgPush={msg}&codCliente={codCliente}", null);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("[CCM] Falha ao enviar push para o cliente {CodCliente}. Status: {Status}", codCliente, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao enviar push para o cliente {CodCliente}", codCliente);
        }
    }

    /// <summary>
    /// Avisa o CCM que o integrador está ativo (mantém a loja online no CCM).
    /// </summary>
    public async Task EnviaActivePingCcmAsync(ClsMerchant merchant)
    {
        try
        {
            var client = _factory.CreateClient("ApiCcm");
            var response = await client.GetAsync($"wsccm.php?token={merchant.TokenCcm}&funcao=activePing&codFilial={merchant.IdTokenCcm}&primeiraVerificacao=1");

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("[CCM] Falha no activePing do merchant {MerchantId}. Status: {Status}", merchant.Id, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro no activePing do merchant {MerchantId}", merchant.Id);
        }
    }

    public async Task LimparPedidoDoPollingCcmAsync(int nroPedido, string tokenCcm)
    {
        try
        {
            var client = _factory.CreateClient("ApiCcm");
            var response = await client.PostAsync($"wsccm_v2.php?token={tokenCcm}&import={nroPedido}", null);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("[CCM] Falha ao limpar o pedido {NroPedido} do polling. Status: {Status}", nroPedido, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CCM] Erro ao limpar o pedido {NroPedido} do polling", nroPedido);
        }
    }

    /// <summary>
    /// Aceite manual de pedido CCM — chamado pelo painel quando o merchant aceita um pedido NOVO.
    /// Aceita no CCM, limpa do polling, envia push ao cliente e atualiza para PREPARANDO no Sophos.
    /// </summary>
    public async Task<bool> AceitarPedidoManualAsync(string idIntegracao, string merchantSophosId)
    {
        if (!int.TryParse(idIntegracao, out var nroPedido))
        {
            _logger.LogWarning("[CCM] AceitarPedidoManual: idIntegracao '{Id}' não é numérico", idIntegracao);
            return false;
        }

        var merchant = await RetornaMerchantCcmAsync(merchantSophosId);
        if (merchant is null)
            return false;

        // O código do cliente (para o push) vem do xml cru salvo no pedido Sophos
        var pedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(idIntegracao);
        int codCliente = RetornaCodClienteDoXml(pedidoSophos?.JsonPedidoDeIntegracao);

        bool aceitou = await AceitarPedidoCcmAsync(nroPedido, merchant.TokenCcm!, codCliente);
        if (!aceitou)
            return false;

        // CCM não tem webhook de status, então atualizamos o pedido para PREPARANDO aqui
        if (pedidoSophos is not null)
            await _nestApiService.UpdatePedidoPreparandoNaAPiPrincipalAsync(null, merchantSophosId, pedidoSophos);

        return true;
    }

    /// <summary>
    /// Recusa manual de pedido CCM — recusa no CCM com a mensagem padrão e cancela o pedido no Sophos.
    /// </summary>
    public async Task<bool> RecusarPedidoManualAsync(string idIntegracao, string merchantSophosId)
    {
        if (!int.TryParse(idIntegracao, out var nroPedido))
        {
            _logger.LogWarning("[CCM] RecusarPedidoManual: idIntegracao '{Id}' não é numérico", idIntegracao);
            return false;
        }

        var merchant = await RetornaMerchantCcmAsync(merchantSophosId);
        if (merchant is null)
            return false;

        bool recusou = await RecusarPedidoCcmAsync(nroPedido, merchant.TokenCcm!);
        if (!recusou)
            return false;

        var pedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(idIntegracao);
        if (pedidoSophos is not null)
            await _nestApiService.UpdatePedidoCanceladodoNaAPiPrincipalAsync(pedidoSophos);

        return true;
    }

    public async Task<bool> DespacharPedidoAsync(UpdatePedidosDto dto)
        => await EnviaUpdateStatusManualAsync(dto, 5);

    public async Task<bool> ConcluirPedidoAsync(UpdatePedidosDto dto)
        => await EnviaUpdateStatusManualAsync(dto, 6);

    private async Task<bool> EnviaUpdateStatusManualAsync(UpdatePedidosDto dto, int valor)
    {
        if (string.IsNullOrEmpty(dto.PedidoIdIntegracao) || !int.TryParse(dto.PedidoIdIntegracao, out var nroPedido))
            return false;

        var merchant = await RetornaMerchantCcmPeloDtoAsync(dto);
        if (merchant is null)
            return false;

        return await AtualizaStatusPedidoCcmAsync(nroPedido, merchant.TokenCcm!, valor);
    }

    /// <summary>
    /// O endpoint público de detalhes do merchant não retorna o TokenCcm,
    /// então buscamos pela mesma lista usada no polling.
    /// </summary>
    private async Task<ClsMerchant?> RetornaMerchantCcmAsync(string merchantSophosId)
    {
        var merchants = await _nestApiService.RetornaMerchantsComIntegracaoCcm();
        var merchant = merchants.FirstOrDefault(m => m.Id == merchantSophosId);
        if (merchant is null || string.IsNullOrEmpty(merchant.TokenCcm))
        {
            _logger.LogWarning("[CCM] Merchant {MerchantId} não encontrado ou sem TokenCcm", merchantSophosId);
            return null;
        }

        return merchant;
    }

    private async Task<ClsMerchant?> RetornaMerchantCcmPeloDtoAsync(UpdatePedidosDto dto)
    {
        string? merchantId = dto.MerchantId;

        if (string.IsNullOrEmpty(merchantId) && !string.IsNullOrEmpty(dto.TokenNestApi))
        {
            var merchantLogado = await _nestApiService.GetMerchantFromNestApi(dto.TokenNestApi);
            if (merchantLogado is not null && !string.IsNullOrEmpty(merchantLogado.TokenCcm))
                return merchantLogado;

            merchantId = merchantLogado?.Id;
        }

        if (string.IsNullOrEmpty(merchantId))
            return null;

        return await RetornaMerchantCcmAsync(merchantId);
    }

    private int RetornaCodClienteDoXml(string? xmlCru)
    {
        if (string.IsNullOrWhiteSpace(xmlCru))
            return 0;

        try
        {
            var serializer = new XmlSerializer(typeof(Pedido));
            using var reader = new StringReader(xmlCru);
            var pedido = serializer.Deserialize(reader) as Pedido;
            return pedido?.Cliente?.Codigo ?? 0;
        }
        catch
        {
            return 0;
        }
    }
    #endregion

    #region Conversão CCM → Ifood
    public PedidoIfoodDto NormalizaPedidoCcmEmPedidoIfood(Pedido pedido)
    {
        string telefone = ApenasDigitos(pedido.Cliente?.Telefone);
        string documento = ApenasDigitos(pedido.PedidoCPF);

        var pedidoIfood = new PedidoIfoodDto
        {
            Id = pedido.NroPedido.ToString(),
            DisplayId = pedido.NroPedido.ToString(),
            OrderType = RetornaOrderType(pedido),
            OrderTiming = pedido.Agendamento == 1 ? "SCHEDULED" : "IMMEDIATE",
            // O CCM retorna o horário local (Brasília); somamos 3h porque a conversão para pedido Sophos subtrai 3h (UTC → Brasília)
            CreatedAt = (ParseDataCcm(pedido.DataHoraPedido) ?? DateTime.Now).AddHours(3),
            ExtraInfo = pedido.ObsGeraisPedido ?? string.Empty,

            Customer = new CustomerIfoodDto
            {
                Id = telefone,
                Name = pedido.Cliente?.Nome ?? string.Empty,
                DocumentNumber = documento,
                // O conversor para pedido Sophos só preenche o Cpf/Cnpj quando o DocumentType bate
                DocumentType = documento.Length == 14 ? "CNPJ" : documento.Length > 0 ? "CPF" : string.Empty,
                Phone = new PhoneIfoodDto
                {
                    Number = telefone,
                    Localizer = telefone,
                }
            },

            Delivery = new DeliveryIfoodDto
            {
                DeliveryAddress = new DeliveryAddresIfoodDto
                {
                    StreetName = pedido.Endereco?.Rua ?? string.Empty,
                    StreetNumber = pedido.Endereco?.Numero ?? string.Empty,
                    Complement = pedido.Endereco?.Complemento ?? string.Empty,
                    Reference = pedido.Endereco?.Referencia ?? string.Empty,
                    Neighborhood = pedido.Endereco?.Bairro ?? string.Empty,
                    City = pedido.Endereco?.Cidade ?? string.Empty,
                    State = pedido.Endereco?.Estado ?? string.Empty,
                    PostalCode = pedido.Endereco?.Cep ?? string.Empty,
                    FormattedAddress = $"{pedido.Endereco?.Rua}, {pedido.Endereco?.Numero} - {pedido.Endereco?.Bairro}".Trim(' ', ',', '-'),
                }
            },
        };

        if (pedido.NumeroMesa > 0)
            pedidoIfood.Indoor = new IndoorIfoodDto { Table = pedido.NumeroMesa.ToString() };

        // Itens e subitens (adicionais)
        foreach (var item in pedido.Itens)
        {
            // O CCM não soma os adicionais no valor total do produto, e os adicionais
            // são por unidade do item: o total é (unitário + adicionais) * quantidade
            double adicionaisPorUnidade = item.Adicionais.Sum(a => (double)a.ValorUnit * a.Quantidade);

            var itemIfood = new ItemIfoodDto
            {
                Id = item.Codigo.ToString(),
                Name = !string.IsNullOrEmpty(item.NomeItem) ? item.NomeItem : item.Descricao ?? string.Empty,
                ExternalCode = item.CodPdv ?? string.Empty,
                Quantity = item.Quantidade,
                UnitPrice = item.ValorUnit,
                TotalPrice = ((double)item.ValorUnit + adicionaisPorUnidade) * item.Quantidade,
                Observations = MontaObsDoItem(item),
                Options = item.Adicionais.Select(a => new OptionsIfoodDto
                {
                    Id = a.Codigo.ToString(),
                    Name = a.Descricao ?? string.Empty,
                    GroupName = a.InfoTipo,
                    ExternalCode = a.CodPdv ?? string.Empty,
                    Quantity = a.Quantidade,
                    UnitPrice = a.ValorUnit,
                    Price = (double)a.ValorUnit * a.Quantidade,
                }).ToList()
            };

            pedidoIfood.Items.Add(itemIfood);
        }

        // Brindes entram como itens de valor zero para sairem na comanda
        foreach (var brinde in pedido.Brindes)
        {
            pedidoIfood.Items.Add(new ItemIfoodDto
            {
                Name = $"BRINDE - {brinde.Descricao}",
                ExternalCode = brinde.CodPdvBrinde ?? string.Empty,
                Quantity = 1,
                UnitPrice = 0,
                TotalPrice = 0,
            });
        }

        // Totais
        double totalDescontos = (double)pedido.ValorCupom + pedido.CreditoUtilizado;
        pedidoIfood.Total = new TotalIfoodDto
        {
            SubTotal = pedido.ValorBruto > 0 ? pedido.ValorBruto : pedidoIfood.Items.Sum(i => i.TotalPrice),
            DeliveryFee = pedido.ValorTaxa,
            Benefits = totalDescontos,
            OrderAmount = pedido.ValorTotal,
        };

        // Descontos (cupom + crédito utilizado)
        if (totalDescontos > 0)
        {
            pedidoIfood.Benefits = new List<BenefitsIfoodDto>
            {
                new BenefitsIfoodDto
                {
                    Description = new List<SponsorShipValuesIfoodDto>
                    {
                        new SponsorShipValuesIfoodDto
                        {
                            Name = "CCM",
                            Description = pedido.CupomDesconto ?? "DESCONTO CCM",
                            Value = totalDescontos,
                        }
                    }
                }
            };
        }

        // Pagamento
        var metodoPagamento = new MethodsIfoodDto
        {
            Method = RetornaTipoDePagamentoIgualDoIfood(pedido.DescricaoPagamento),
            Type = pedido.PagamentoOnline == 1 ? "ONLINE" : "OFFLINE",
            Value = pedido.ValorTotal,
        };

        double trocoPara = ParseValorCcm(pedido.TrocoPara);
        if (trocoPara > 0)
        {
            // Convenção iFood: Value = total do pedido, ChangeFor = valor pago pelo cliente
            // Assim: Troco = ChangeFor - Value (calculado em IfoodServices)
            metodoPagamento.Cash = new CashMethodsIfoodDto { ChangeFor = trocoPara };
        }

        pedidoIfood.Payments = new PaymentIfoodDto
        {
            Methods = new List<MethodsIfoodDto> { metodoPagamento }
        };

        // Agendamento
        if (pedidoIfood.OrderTiming == "SCHEDULED")
        {
            DateTime? dataAgendamento = ParseDataCcm(pedido.DataHoraAgendamento)
                ?? ParseDataCcm($"{pedido.DataAgendamento} {pedido.HoraAgendamento}".Trim());

            if (dataAgendamento != null)
            {
                // +3h pelo mesmo motivo do CreatedAt: a conversão para pedido Sophos subtrai 3h
                if (pedidoIfood.OrderType == "DELIVERY")
                    pedidoIfood.Delivery.DeliveryDateTime = dataAgendamento.Value.AddHours(3);
                else if (pedidoIfood.OrderType == "TAKEOUT")
                    pedidoIfood.Takeout.TakeoutDateTime = dataAgendamento.Value.AddHours(3);
            }
        }

        return pedidoIfood;
    }

    private string RetornaOrderType(Pedido pedido)
    {
        if (pedido.NumeroMesa > 0)
            return "INDOOR";

        if (pedido.Retira == 1)
            return "TAKEOUT";

        return "DELIVERY";
    }

    private string MontaObsDoItem(Item item)
    {
        var obs = new StringBuilder(item.ObsItem ?? string.Empty);

        foreach (var parte in item.Parte)
        {
            if (!string.IsNullOrWhiteSpace(parte.ObsParte))
            {
                if (obs.Length > 0)
                    obs.Append(" | ");
                obs.Append(parte.ObsParte);
            }
        }

        return obs.ToString();
    }

    private string RetornaTipoDePagamentoIgualDoIfood(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return "CASH";

        var n = descricao.Trim().ToUpperInvariant()
                    .Replace("É", "E").Replace("Ê", "E")
                    .Replace("Ã", "A").Replace("Á", "A").Replace("Â", "A")
                    .Replace("Ç", "C").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");

        if (n.Contains("PIX")) return "PIX";
        if (n.Contains("CREDITO") || n.Contains("CREDIT")) return "CREDIT";
        if (n.Contains("DEBITO") || n.Contains("DEBIT")) return "DEBIT";
        if (n.Contains("DINHEIRO") || n.Contains("MONEY") || n.Contains("CASH")) return "CASH";
        if (n.Contains("REFEI")) return "MEAL_VOUCHER";
        if (n.Contains("ALIMENT")) return "FOOD_VOUCHER";
        if (n.Contains("ONLINE")) return "DIGITAL_WALLET";

        return "CASH"; // fallback
    }

    private static DateTime? ParseDataCcm(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        string[] formatos =
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy",
        };

        if (DateTime.TryParseExact(valor.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        if (DateTime.TryParse(valor, new CultureInfo("pt-BR"), DateTimeStyles.None, out dt))
            return dt;

        return null;
    }

    private static double ParseValorCcm(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return 0;

        // O CCM envia o valor com ponto decimal ("50.00"); em pt-BR o ponto é separador
        // de milhar, então trocamos por vírgula antes de parsear (mesmo tratamento da WinForms)
        valor = valor.Replace("R$", "").Trim().Replace(".", ",");

        if (double.TryParse(valor, NumberStyles.Any, new CultureInfo("pt-BR"), out var v))
            return v;

        return 0;
    }

    private static string ApenasDigitos(string? valor) =>
        string.IsNullOrEmpty(valor) ? string.Empty : Regex.Replace(valor, @"\D", "");
    #endregion
}
