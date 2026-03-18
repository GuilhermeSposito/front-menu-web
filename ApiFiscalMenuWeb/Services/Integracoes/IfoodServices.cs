using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using FrontMenuWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using Unimake.Business.DFe.Servicos;
using Unimake.MessageBroker.Primitives.Contract.Response;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class IfoodServices
{
    #region Propriedades
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiService;
    private readonly ILogger<IfoodServices> _logger;
    private readonly EmailService _emailService;
    private List<PollingIfoodDto> PollingsToAcknowledge = new List<PollingIfoodDto>();
    private readonly IConfiguration _configuration;

    public IfoodServices(IHttpClientFactory factory, NestApiServices nestApiService, ILogger<IfoodServices> logger, EmailService emailService, IConfiguration configuration)
    {
        _factory = factory;
        _nestApiService = nestApiService;
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }
    #endregion

    public async Task<bool> AutenticarEmpresa()
    {
        string? ClientIdIfood = _configuration["Ifood:ClientId"];
        string? ClientSercretIfood = _configuration["Ifood:ClientSecret"];

        if (string.IsNullOrEmpty(ClientIdIfood) || string.IsNullOrEmpty(ClientSercretIfood))
            throw new Exception("ClientId ou ClientSecret do ifood não encontrado nas variáveis de ambiente");


        var HttpIfood = _factory.CreateClient("ApiIfoodAuth");
        FormUrlEncodedContent formDataToGetTheToken = new FormUrlEncodedContent(new[]
        {
              new KeyValuePair<string, string>("grantType", "client_credentials"),
              new KeyValuePair<string, string>("clientId", ClientIdIfood),
              new KeyValuePair<string, string>("clientSecret", ClientSercretIfood),
            });

        var response = await HttpIfood.PostAsync("/authentication/v1.0/oauth/token", formDataToGetTheToken);
        var result = await response.Content.ReadFromJsonAsync<InformacoesDoTokenRetornadaPeloIfoodDto>();

        if (result is not null)
        {
            Environment.SetEnvironmentVariable("TOKEN_IFOOD_REQS", result.AccessToken, EnvironmentVariableTarget.Process);
            return true;
        }

        return false;
    }

    #region Pooling E Add Pedido Region
    public async Task PollingIfood()
    {
        try
        {
            Console.WriteLine("Fez polling");

            var ifoodClient = _factory.CreateClient("ApiIfood");
            var ResponsePolling = await ifoodClient.GetAsync("/events/v1.0/events:polling");

            if (ResponsePolling.IsSuccessStatusCode)
            {
                var PollingResult = await ResponsePolling.Content.ReadFromJsonAsync<List<PollingIfoodDto>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (PollingResult is null || PollingResult.Count() == 0)
                    return;

                foreach (var Polling in PollingResult)
                {
                    var WebHookDto = new WebHookIfoodDto
                    {
                        FullCode = Polling.FullCode,
                        CreatedAt = Polling.CreatedAt,
                        Code = Polling.Code,
                        MerchantId = Polling.MerchantId,
                        OrderId = Polling.OrderId
                    };

                    await AddOrUpdateOrders(WebHookDto);
                }


            }
        }
        catch (Exception ex)
        {
            await EnviaEmailDeErro(ex.ToString());
            _logger.LogError(ex, "Erro no pooling do iFood. Endpoint: {PollingIfood}", "Pooling");
            return;
        }
    }

    public async Task<ReturnApiRefatored<object>> AddOrUpdateOrders(WebHookIfoodDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.MerchantId))
                return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "MerchantId não pode ser nulo ou vazio" } };

            List<string> Messages = new List<string>();

            ClsEmpresaIfood? Empresa = await _nestApiService.RetornaEmpresaIfoodPeloMerchantId(dto.MerchantId);
            if (Empresa is null || Empresa.MerchantSophos is null)
                throw new Exception("Merchant não encontrado na base de dados do sophos");


            switch (dto.Code)
            {
                case "PLC": //caso entre aqui é porque é um novo pedido     
                    string MensagemDeTentativaDeAddPedido = await AdicionaPedidoAoSophos(dto.OrderId, Empresa.MerchantSophos, dto, Empresa);
                    Messages.Add(MensagemDeTentativaDeAddPedido);
                    break;
                case "CFM":
                    //Aqui qunado for aceito o pedido e vier a confimação
                    break;
                case "DSP":
                    await MudaStatusPedidoDespachado(new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, MerchantId = Empresa.MerchantSophos.Id, PedidoIdIntegracao = dto.OrderId });
                    break;
                case "CON":
                    await MudaStatusPedidoConcluido(new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, MerchantId = Empresa.MerchantSophos.Id, PedidoIdIntegracao = dto.OrderId });
                    break;
                case "CAN":
                    await MudaStatusPedidoCancelado(new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, MerchantId = Empresa.MerchantSophos.Id, PedidoIdIntegracao = dto.OrderId });
                    break;
                default:
                    await MudaStatusComoINfosAdicionaisPedidoNaAPiPrincipal(new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, MerchantId = Empresa.MerchantSophos.Id, PedidoIdIntegracao = dto.OrderId }, dto);
                    break;
            }

            return new ReturnApiRefatored<object> { Status = "success", Messages = new List<string> { "Pedido processado com Sucesso!" } };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Conversão do pedido Ifood não válida. Endpoint: {AddOrUpdateOrder}", "Pooling");
            await EnviaEmailDeErro(ex.ToString());
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Não Foi possivel ler o pedido do ifood" } };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Pooling do iFood cancelado. Endpoint: {AddOrUpdateOrder}", "Pooling");
            await EnviaEmailDeErro(ex.ToString());
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "A requisição para leitura de pedidos demorou muito e foi cancelada." } };
        }
        catch (Exception ex)
        {
            _logger.LogError("Tipo real da exception: {Type}", ex.GetType().FullName);
            await EnviaEmailDeErro(ex.ToString());
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao ler pedidos ifood" } };
        }

    }
    #endregion

    #region Funções de Ação dos Pedidos Do Ifood
    private async Task<PedidoIfoodDto?> GetPedidoIfood(string OrderId, ClsEmpresaIfood MerchantIfood)
    {
        var IfoodClient = _factory.CreateClient("ApiIfood");

        var PedidoResponse = await IfoodClient.GetAsync($"order/v1.0/orders/{OrderId}");
        if (PedidoResponse.IsSuccessStatusCode)
        {
            int StatusCode = (int)PedidoResponse.StatusCode;
            if (StatusCode != 200)
                return null;

            PedidoIfoodDto? PedidoIfood = JsonSerializer.Deserialize<PedidoIfoodDto>(await PedidoResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return PedidoIfood;
        }
        else
        {
            return null;
        }
    }

    private async Task<bool> AceitaPedido(string PedidoId, ClsEmpresaIfood MerchantIfood)
    {
        var IfoodClient = _factory.CreateClient("ApiIfood");

        var ResponseConfirm = await IfoodClient.PostAsync($"order/v1.0/orders/{PedidoId}/confirm", null);
        return ResponseConfirm.IsSuccessStatusCode;
    }

    public async Task<bool> MudaStatusPedidoDespachado(UpdatePedidosDto UpdateDto, PollingIfoodDto? Polling = null)
    {
        HttpClient? HttpIntegracaoCliente = null;
        if (UpdateDto.DestinoPedido == DestinoPedido.Ifood)
        {
            HttpIntegracaoCliente = _factory.CreateClient("ApiIfood");
            string PedidoIdIfood = UpdateDto.Pedido?.IfoodID ?? UpdateDto.PedidoIdIntegracao;
            var response = await HttpIntegracaoCliente.PostAsync($"order/v1.0/orders/{PedidoIdIfood}/dispatch", null);
            if (Polling is not null)
            {
                if (response.IsSuccessStatusCode)
                    PollingsToAcknowledge.Add(Polling);
            }
        }

        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos && UpdateDto.TokenNestApi is not null)
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                var response = await _nestApiService.UpdatePedidoDespachadoNaAPiPrincipalAsync(UpdateDto.TokenNestApi, UpdateDto.MerchantId, PedidoSophos);


            }
        }



        return true;
    }

    public async Task<bool> MudaStatusPedidoConcluido(UpdatePedidosDto UpdateDto, PollingIfoodDto? Polling = null)
    {
        HttpClient? HttpIntegracaoCliente = null;

        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos && (UpdateDto.TokenNestApi is not null || UpdateDto.MerchantId is not null))
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                var response = await _nestApiService.UpdatePedidoConcluidodoNaAPiPrincipalAsync(UpdateDto.TokenNestApi, UpdateDto.MerchantId, PedidoSophos);

            }
        }

        return true;
    }

    public async Task<bool> MudaStatusPedidoCancelado(UpdatePedidosDto UpdateDto)
    {
        HttpClient? HttpIntegracaoCliente = null;
        if (UpdateDto.DestinoPedido == DestinoPedido.Ifood)
        {
            HttpIntegracaoCliente = _factory.CreateClient("ApiIfood");
            string PedidoIdIfood = UpdateDto.Pedido?.IfoodID ?? UpdateDto.PedidoIdIntegracao;
        }

        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos && UpdateDto.MerchantId is not null)
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                var response = await _nestApiService.UpdatePedidoCanceladodoNaAPiPrincipalAsync(PedidoSophos);
            }

        }

        return true;
    }
    public async Task<bool> MudaStatusComoINfosAdicionaisPedidoNaAPiPrincipal(UpdatePedidosDto UpdateDto, WebHookIfoodDto Polling)
    {
        HttpClient? HttpIntegracaoCliente = null;
        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos)
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                string infoAdicional = RetornaStatusCompletoAtualizado(Polling.Code);

                if (!string.IsNullOrEmpty(infoAdicional) && !string.IsNullOrEmpty(UpdateDto.MerchantId))
                    await _nestApiService.UpdatePedidoInfosAdicionaisOuStatusoNaAPiPrincipalAsync(UpdateDto.MerchantId, PedidoSophos, new UpdatePedidoInfosAdicionaisDto { InfoAdicionalOuStatus = infoAdicional });
            }
        }

        return true;
    }

    public async Task<ReturnApiRefatored<object>> EnviaCancelamentoDePedido(string TokenNestApi, CancelationIfoodObjectDto CancelationDto)
    {
        try
        {
            ClsMerchant? Merchant = await _nestApiService.GetMerchantFromNestApi(TokenNestApi);
            if (Merchant is null)
                throw new Exception("Não Foi possivel obter acesso as informações do estabelecimento!");

            ClsPedido? PedidoReferente = CancelationDto.Pedido;

            ClsEmpresaIfood? EmpresaIfood = null;
            if (Merchant.EmpresasIfood.Count == 1)
                EmpresaIfood = Merchant.EmpresasIfood.FirstOrDefault();

            if (EmpresaIfood is null)
            {
                try
                {
                    if (PedidoReferente is null)
                        throw new Exception("Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento");

                    PedidoIfoodDto? PedidoIfood = JsonSerializer.Deserialize<PedidoIfoodDto>(PedidoReferente.JsonPedidoDeIntegracao ?? " ", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (PedidoIfood is null)
                        throw new Exception("Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento");

                    EmpresaIfood = Merchant.EmpresasIfood.FirstOrDefault(e => e.MerchantIdIfood == PedidoIfood.Merchant.MerchantId);
                    if (EmpresaIfood is null)
                        throw new Exception("Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento");
                }
                catch (Exception ex)
                {
                    return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento" } };
                }
            }
            var IfoodClient = _factory.CreateClient("ApiIfood");

            var json = JsonSerializer.Serialize(CancelationDto.CancalationComfirmation);

            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var Response = await IfoodClient.PostAsync(
                $"order/v1.0/orders/{PedidoReferente?.IfoodID}/requestCancellation",
                content
            );

            if (Response.IsSuccessStatusCode)
            {

                return new ReturnApiRefatored<object> { Status = "success", Data = new Data<object> { Messages = new List<string> { "Motivo de cancelamento enviado com sucesso para o ifood!" } } };
            }
            else
            {
                return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao enviar o motivo de cancelamento para o ifood" } };
            }

        }
        catch (Exception ex)
        {
            await EnviaEmailDeErro(ex.ToString());
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao obter os motivos de cancelamento do ifood" } };
        }

    }
    #endregion

    #region Funções de Conversão de Pedido Ifood para Pedido do SOPHOS
    private async Task<ClsPedido?> ConvertePedidoDoIfoodParaPedidoSophos(PedidoIfoodDto PedidoIfood, ClsMerchant MerchantSophos)
    {
        ClsPedido PedidoSophos = new ClsPedido
        {
            IfoodID = PedidoIfood.Id,
            CriadoEm = PedidoIfood.CreatedAt.AddHours(-3), // Porque é retornado o horário em UTC, então subtraio 3 horas para converter para horário de Brasília
            CriadoPor = "IFOOD",
            TipoDePedido = RetornaTipoDoPedidoSophos(PedidoIfood.OrderType),
            EtapaPedido = "PREPARANDO",
            DisplayId = PedidoIfood.DisplayId,
            StatusPedido = "FECHADO",
            ValorDosItens = (float)PedidoIfood.Total.SubTotal,
            ValorTotal = (float)PedidoIfood.Total.OrderAmount,
            AcrescimoValor = (float)PedidoIfood.Total.AddtionalFees,
            TaxaEntregaValor = (float)PedidoIfood.Total.DeliveryFee,
            ObservacaoDoPedido = PedidoIfood.ExtraInfo
        };

        PedidoSophos.Itens = await RetornaItensSophos(PedidoIfood.Items, MerchantSophos.Id);

        //Definir Pagamento
        PedidoSophos.Pagamentos = RetornaPagamentosDoPedido(PedidoIfood, PedidoSophos, MerchantSophos);
        //Definir o clitente 
        PedidoSophos.Cliente = RetornaClienteSophos(PedidoIfood.Customer);
        //Definir o Endereço de Entrega
        PedidoSophos.Endereco = RetornaEnderecoDoClienteSophos(PedidoIfood.Delivery.DeliveryAddress);

        var json = JsonSerializer.Serialize(PedidoIfood);
        PedidoSophos.JsonPedidoDeIntegracao = json;

        //DEFINIR PEDIDOS AGENDADOS
        if (PedidoIfood.OrderTiming == "SCHEDULED")
        {
            PedidoSophos.PedidoAgendado = true;
            if (PedidoIfood.OrderType == "DELIVERY")
            {
                PedidoSophos.HorarioDataAgendamento = PedidoIfood.Delivery.DeliveryDateTime.AddHours(-3);
            }
            else if (PedidoIfood.OrderType == "TAKEOUT")
            {
                PedidoSophos.HorarioDataAgendamento = PedidoIfood.Takeout.TakeoutDateTime.AddHours(-3);
            }
        }

        return PedidoSophos;
    }

    private async Task<List<ItensPedido>> RetornaItensSophos(List<ItemIfoodDto> ItensIfood, string MerchantSophosId)
    {
        //Fazer integração depois para buscar o produto no banco de dados do sophos e preencher o Id do produto e o Id do preço, por enquanto vai ser só a descrição mesmo

        var ItensSophos = new List<ItensPedido>();

        foreach (var item in ItensIfood)
        {
            var ResultadoConversaoCodPdv = EditaCodigoPdvIfoodParaPadroSophos(item);

            ClsProduto? ProdutoSophos = await _nestApiService.RetornaProdutoEncontrado(ResultadoConversaoCodPdv.Codigo, MerchantSophosId);
            ItensPedido ItemSophos = new ItensPedido
            {
                Descricao = item.Name,
                Quantidade = (float)item.Quantity,
                PrecoUnitario = (float)item.UnitPrice,
                PrecoTotal = (float)item.TotalPrice,
                Observacoes = item.Observations,
                LegTamanhoEscolhido = ResultadoConversaoCodPdv.Legenda
            };

            //Aqui se o produto foi encontrado
            if (ProdutoSophos is not null)
            {
                ItemSophos.ProdutoId = ProdutoSophos.Id;
                ItemSophos.Produto = ProdutoSophos;
                ItemSophos.Descricao = ProdutoSophos.Descricao;
            }

            foreach (var complemento in item.Options)
            {
                ClsComplemento? ComplementoSophosEncontrado = await _nestApiService.RetornaComplementoEncontrado(complemento.ExternalCode);

                ComplementoNoItem ComplementoSophos = new ComplementoNoItem
                {
                    Descricao = complemento.Name,
                    Quantidade = (float)complemento.Quantity,
                    PrecoUnitario = (float)complemento.UnitPrice,
                    PrecoTotal = (float)complemento.Price,
                };

                if (ComplementoSophosEncontrado is not null)
                {
                    ComplementoSophos.ComplementoId = ComplementoSophosEncontrado.Id;
                    ComplementoSophos.Complemento = ComplementoSophosEncontrado;
                    ComplementoSophos.Descricao = ComplementoSophosEncontrado.Descricao;
                }

                ItemSophos.Complementos.Add(ComplementoSophos);
            }

            ItensSophos.Add(ItemSophos);
        }

        return ItensSophos;
    }

    private List<PagamentoDoPedido> RetornaPagamentosDoPedido(PedidoIfoodDto PedidoIfood, ClsPedido PedidoSophosAtual, ClsMerchant MerchantSophos)
    {
        //Primeiro Definir os Descontos, (Se foram descontos do Ifood Ou se Foram Descontos do Merchant)
        foreach (var Desconto in PedidoIfood.Benefits)
        {
            float ValorTotalEmDesconto = 0f;
            float ValorTotalEmIncentivoExterno = 0f;
            foreach (var InformacoesDoDesconto in Desconto.Description)
            {
                if (InformacoesDoDesconto.Name == "IFOOD")
                {
                    ValorTotalEmIncentivoExterno += (float)InformacoesDoDesconto.Value;
                }
                else
                {
                    ValorTotalEmDesconto += (float)InformacoesDoDesconto.Value;
                }
            }

            PedidoSophosAtual.DescontoValor += ValorTotalEmDesconto;
            PedidoSophosAtual.IncentivosExternosValor += ValorTotalEmIncentivoExterno;
        }

        //Definir os Pagamentos
        var PagamentosDoPedido = new List<PagamentoDoPedido>();
        foreach (var Metodo in PedidoIfood.Payments.Methods)
        {
            PagamentoDoPedido Pagamento = new PagamentoDoPedido
            {
                ValorTotal = (float)Metodo.Value,
                CriadoEm = DateTime.Now
            };

            if (Metodo.Type == "ONLINE")
            {
                var PagamentosOnline = MerchantSophos.FormasDeRecebimento.Where(f => f.PagamentoOnline).ToList();
                Pagamento.FormaDePagamento = RetornaTipoDePagamentoQueBataComCodeDoCardapioIntegrado(PagamentosOnline, Metodo.Method, PagOnline: true) ?? PagamentosOnline.FirstOrDefault();
            }
            else
            {
                Pagamento.FormaDePagamento = RetornaTipoDePagamentoQueBataComCodeDoCardapioIntegrado(MerchantSophos.FormasDeRecebimento, Metodo.Method) ?? MerchantSophos.FormasDeRecebimento.FirstOrDefault();
            }

            if (Metodo.Type == "CASH")
            {
                var Troco = Metodo.Cash.ChangeFor - Metodo.Value;
                Pagamento.Troco = (float)Troco;
            }

            Pagamento.formaDeRecebimentoId = Pagamento.FormaDePagamento?.Id ?? 0;
            PagamentosDoPedido.Add(Pagamento);
        }

        return PagamentosDoPedido;
    }

    public ClsPessoas? RetornaClienteSophos(CustomerIfoodDto? ClienteIfood)
    {
        if (ClienteIfood is null)
            return null;

        return new ClsPessoas
        {
            Nome = ClienteIfood.Name,
            Cpf = ClienteIfood.DocumentType == "CPF" ? ClienteIfood.DocumentNumber : null,
            Cnpj = ClienteIfood.DocumentType == "CNPJ" ? ClienteIfood.DocumentNumber : null,
            Telefone = ClienteIfood.Phone?.Localizer
        };
    }

    public EnderecoPessoa? RetornaEnderecoDoClienteSophos(DeliveryAddresIfoodDto? EnderecoIfood)
    {
        if (EnderecoIfood is null)
            return null;

        return new EnderecoPessoa
        {
            Rua = EnderecoIfood.StreetName,
            Numero = EnderecoIfood.StreetNumber,
            Bairro = EnderecoIfood.Neighborhood,
            Complemento = EnderecoIfood.Complement,
            Referencia = EnderecoIfood.Reference,
            Cep = EnderecoIfood.PostalCode,
            Cidade = EnderecoIfood.City,
            Estado = EnderecoIfood.State,
            ObsEndereco = EnderecoIfood.FormattedAddress,
            TipoEndereco = "ENTREGA IFOOD"
        };
    }

    private async Task<string> AdicionaPedidoAoSophos(string OrderId, ClsMerchant Merchant, WebHookIfoodDto P, ClsEmpresaIfood MerchantIfood)
    {
        PedidoIfoodDto? PedidoIFood = await GetPedidoIfood(OrderId, MerchantIfood);
        if (PedidoIFood is null)
        {
            return $"Falha ao obter os detalhes do pedido {OrderId} para o merchant {Merchant.NomeFantasia}";

        }

        ClsPedido? PedidoSophos = await ConvertePedidoDoIfoodParaPedidoSophos(PedidoIFood, Merchant);
        if (PedidoSophos is null)
        {
            return $"Falha ao converter o pedido {OrderId} para o formato do Sophos para o merchant {Merchant.NomeFantasia}";
        }

        bool AdicionouOPedido = await _nestApiService.CriarPedidoSophos(Merchant, PedidoSophos);
        if (AdicionouOPedido)
        {
            if (Merchant.AceitaPedidoAutDeIntegracoes) //Aqui serve para podermos integrar com a loja do cliente mas não aceitar os pedidos pra ele, apenas visualizar 
            {
                bool AceitouPedido = await AceitaPedido(PedidoIFood.Id, MerchantIfood);
                if (AceitouPedido)
                    return $"Pedido {OrderId} processado e adicionado com sucesso para o merchant {Merchant.NomeFantasia}";
            }
            return $"Pedido {OrderId}  {Merchant.NomeFantasia}";
        }
        else
        {
            return $"Falha ao adicionar o pedido {P.OrderId} para o merchant {Merchant.NomeFantasia}";
        }
    }
    #endregion

    #region Funções Auxiliares
    private string RetornaTipoDoPedidoSophos(string tipoIfood)
    {
        switch (tipoIfood)
        {
            case "DELIVERY":
                return "DELIVERY";
            case "TAKEOUT":
                return "BALCÃO";
            case "INDOOR":
                return "MESA";
            case "DINE_IN":
                return "BALCÃO";
            default:
                return "BALCÃO";
        }
    }

    private string? RetornaTamanhoDoItem(string legendaTamnho)
    {
        switch (legendaTamnho)
        {
            case "P":
                return "PEQUENO";
            case "M":
                return "MÉDIO";
            case "G":
                return "GRANDE";
            case "B":
                return "BROTINHO";
            case "LAN":
                return "LANCHE";
            case "PRC":
                return "PORÇÃO";
            case "C":
                return "CENTO";
            case "MC":
                return "MEIO CENTO";
            case "F":
                return "FOGAZZA";
            case "MIN":
                return "MINI";
            case "S":
                return "SUPER";
            default:
                return null;
        }
    }

    private string RetornaStatusCompletoAtualizado(string legendaTamnho)
    {
        switch (legendaTamnho) //Atualiza status enviados pelo ifood
        {
            case "CAR":
                return "Solicitação de cancelamento feita pelo Merchant (loja) ou pelo iFood de maneira automática (pedidos não confirmados dentro do prazo) ou de maneira manual através do nosso time de atendimento";
            case "CARF":
                return "Solicitação de cancelamento negada";
            case "HSD":
                return "Pedido com solicitação de alteração ou cancelamento. Entre em contato com o cliente para negociar.";
            case "HSS":
                return "Disputa Resolvida e Formalizada";
            case "DAR":
                return "Cliente solicitou alteração do endereço de entrega do pedido";
            case "DAU":
                return "Cliente confirmou o endereço de entrega";
            case "DAA":
                return "A alteração do endereço de entrega foi aprovada pelo parceiro";
            case "DAD":
                return "A alteração do endereço de entrega foi negada pelo parceiro";
            case "CLT":
                return "Entregador coletou o pedido";
            case "AAD":
                return "Entregador chegou no endereço de destino";
            case "DRGO":
                return "Entregador está retornando ao local de origem(coleta) do pedido";
            case "DRDO":
                return "Entregador já retornou ao local de origem(coleta) do pedido";
            case "DCR":
                return "Solicitação de cancelamento de entregador";
            case "DDCS":
                return "O código de confirmação de entrega foi validado com sucesso";
            case "DPCS":
                return "O código de confirmação de coleta foi validado com sucesso";
            case "DPCR":
                return "Informe o codigo de coleta na entrega do pedido!";
            default:
                return string.Empty;
        }
    }

    private (string? Codigo, string? Legenda) EditaCodigoPdvIfoodParaPadroSophos(ItemIfoodDto itemIfoodDto)
    {
        if (string.IsNullOrEmpty(itemIfoodDto.ExternalCode))
            return (null, null);

        bool eCodigoSimples = int.TryParse(itemIfoodDto.ExternalCode, out _);
        if (eCodigoSimples)
            return (itemIfoodDto.ExternalCode, null);

        List<string> legendasExistentes = ["G", "M", "P", "B", "LAN", "PRC", "C", "MC", "F", "MIN", "S"];

        foreach (var legenda in legendasExistentes)
        {
            if (itemIfoodDto.ExternalCode.Contains(legenda))
            {
                var codigoLimpo = itemIfoodDto.ExternalCode.Replace(legenda, "");
                string? LegendaFormatada = RetornaTamanhoDoItem(legenda);
                return (codigoLimpo, LegendaFormatada);
            }
        }

        return (itemIfoodDto.ExternalCode, null);
    }

    private ClsFormaDeRecebimento? RetornaTipoDePagamentoQueBataComCodeDoCardapioIntegrado(List<ClsFormaDeRecebimento> formas, string CodeIfood, bool PagOnline = false)
    {
        if (formas == null || !formas.Any() || string.IsNullOrWhiteSpace(CodeIfood))
            return null;

        CodeIfood = CodeIfood.Trim().ToUpperInvariant();

        if (PagOnline)
        {
            var FormaDePagamentoOnlineIfood = formas.FirstOrDefault(f => f.Descricao.Contains("ONLINE IFOOD", StringComparison.OrdinalIgnoreCase));
            if (FormaDePagamentoOnlineIfood is not null)
                return FormaDePagamentoOnlineIfood;
        }

        return CodeIfood switch
        {
            "DIGITAL_WALLET" => formas.FirstOrDefault(f => f.Descricao.Contains("ONLINE", StringComparison.OrdinalIgnoreCase) || f.Descricao.Contains("ONLINE IFOOD", StringComparison.OrdinalIgnoreCase)),

            "CASH" => formas.FirstOrDefault(f => f.EDinheiro || f.Descricao.Contains("DINHEIRO", StringComparison.OrdinalIgnoreCase)),

            "CREDIT" => formas.FirstOrDefault(f => f.ECredito || f.Descricao.Contains("CRÉDITO", StringComparison.OrdinalIgnoreCase) || f.Descricao.Contains("CREDITO", StringComparison.OrdinalIgnoreCase)),

            "DEBIT" => formas.FirstOrDefault(f => f.EDEbito || f.Descricao.Contains("DÉBITO", StringComparison.OrdinalIgnoreCase) || f.Descricao.Contains("DEBITO", StringComparison.OrdinalIgnoreCase)),

            "MEAL_VOUCHER" => formas.FirstOrDefault(f => f.Descricao.Contains("REFEI", StringComparison.OrdinalIgnoreCase)),

            "FOOD_VOUCHER" => formas.FirstOrDefault(f => f.Descricao.Contains("ALIMENT", StringComparison.OrdinalIgnoreCase)),

            "GIFT_CARD" => formas.FirstOrDefault(f => f.Descricao.Contains("PRESENTE", StringComparison.OrdinalIgnoreCase) || f.Descricao.Contains("GIFT", StringComparison.OrdinalIgnoreCase)),


            "PIX" => formas.FirstOrDefault(f => f.EPix || f.Descricao.Contains("PIX", StringComparison.OrdinalIgnoreCase)),

            _ => formas.FirstOrDefault()
        };
    }

    public async Task<ReturnApiRefatored<ClsCancelationReasons>> GetCanceletionReasons(string TokenNestApi, string IdPedidoIfood)
    {
        try
        {
            ClsMerchant? Merchant = await _nestApiService.GetMerchantFromNestApi(TokenNestApi);
            if (Merchant is null)
                throw new Exception("Não Foi possivel obter acesso as informações do estabelecimento!");

            ClsPedido? PedidoReferente = null;

            ClsEmpresaIfood? EmpresaIfood = null;
            if (Merchant.EmpresasIfood.Count == 1)
                EmpresaIfood = Merchant.EmpresasIfood.FirstOrDefault();

            if (EmpresaIfood is null)
            {
                try
                {
                    PedidoReferente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(IdPedidoIfood);
                    if (PedidoReferente is null)
                        throw new Exception("Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento");

                    PedidoIfoodDto? PedidoIfood = JsonSerializer.Deserialize<PedidoIfoodDto>(PedidoReferente.JsonPedidoDeIntegracao ?? " ", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (PedidoIfood is null)
                        throw new Exception("Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento");

                    EmpresaIfood = Merchant.EmpresasIfood.FirstOrDefault(e => e.MerchantIdIfood == PedidoIfood.Merchant.MerchantId);
                    if (EmpresaIfood is null)
                        throw new Exception("Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento");
                }
                catch (Exception ex)
                {
                    return new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Não Foi possivel ler os dados do pedido referente para obter o motivo de cancelamento" } };
                }

            }

            var IfoodClient = _factory.CreateClient("ApiIfood");

            var Response = await IfoodClient.GetAsync($"order/v1.0/orders/{PedidoReferente?.IfoodID ?? IdPedidoIfood}/cancellationReasons");
            List<ClsCancelationReasons>? CancelationReasons = await Response.Content.ReadFromJsonAsync<List<ClsCancelationReasons>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (CancelationReasons is null)
                return new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Erro ao obter os motivos de cancelamento do ifood" } };


            return new ReturnApiRefatored<ClsCancelationReasons> { Status = "success", Messages = new List<string> { "Motivos De cancelamento encontrado com sucesso!" }, Data = new Data<ClsCancelationReasons> { ListWhenWriting = CancelationReasons, Messages = new List<string> { "Motivos De cancelamento encontrado com sucesso!" } } };
        }
        catch (Exception ex)
        {
            await EnviaEmailDeErro(ex.ToString());
            return new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Erro ao obter os motivos de cancelamento do ifood" } };
        }


    }

    public async Task EnviaEmailDeErro(string erro)
    {
        var html = $"""
                <div style="font-family: Arial, sans-serif; background-color:#f4f4f4; padding:20px;">
        
                    <div style="max-width:800px; margin:auto; background:white; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.1);">
            
                        <div style="background-color:#c62828; color:white; padding:15px;">
                            <h2 style="margin:0;">🚨 Erro na API De Integrações</h2>
                        </div>

                        <div style="padding:20px; color:#333;">
                
                            <p><strong>Data:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                            <p><strong>Servidor:</strong> {Environment.MachineName}</p>

                            <hr style="margin:20px 0;" />

                            <h3 style="color:#c62828;">Detalhes do Erro</h3>

                            <pre style="
                                background:#1e1e1e;
                                color:#FFFFFF;
                                padding:15px;
                                border-radius:5px;
                                overflow:auto;
                                font-size:13px;
                            ">

                            {erro}
                            </pre>

                        </div>
                    </div>
                </div>
                """;

        await _emailService.EnviarAsync(
            "guilherme@sophos-erp.com.br",
            $"Erro de Exceção na API De Integrações {DateTime.Now:g}",
            html
        );
    }
    #endregion
}


