using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
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
    private List<PollingIfoodDto> PollingsToAcknowledge = new List<PollingIfoodDto>();

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

    public async Task<ReturnApiRefatored<object>> AutenticarEmpresa(InformacoesParaAutenticarEmpresaIfoodDto? Infos, string TokenNestAPi, bool Refresh, string? RefreshToken, int IdEmpresa)
    {
        var HttpIfood = _factory.CreateClient("ApiIfood");
        FormUrlEncodedContent formDataToGetTheToken = new FormUrlEncodedContent(new[]
        {
              new KeyValuePair<string, string>("grantType", "authorization_code"),
              new KeyValuePair<string, string>("clientId", "7e476dce-79fa-4a7e-a605-aa2a1a40b803"),
              new KeyValuePair<string, string>("clientSecret", "z5086yxoeeblv5go12ag9ynk2i8oan36l0gca8y9vs0h66yrorjh2nccdmxpbxk955lb0j6wc7vdpb2i3416aqs8ja4xjhbw3u0"),
              new KeyValuePair<string, string>("authorizationCode", Infos?.CodigoDeAutorizacaoEnviadoPeloIfood ?? ""),
              new KeyValuePair<string, string>("authorizationCodeVerifier", Infos?.VerificadorDoCodigo ?? "")
        });

        if (Refresh && !string.IsNullOrEmpty(RefreshToken))
        {
            formDataToGetTheToken = new FormUrlEncodedContent(new[]
             {
                        new KeyValuePair<string, string>("grantType", "refresh_token"),
                        new KeyValuePair<string, string>("clientId", "7e476dce-79fa-4a7e-a605-aa2a1a40b803"),
                        new KeyValuePair<string, string>("clientSecret", "z5086yxoeeblv5go12ag9ynk2i8oan36l0gca8y9vs0h66yrorjh2nccdmxpbxk955lb0j6wc7vdpb2i3416aqs8ja4xjhbw3u0"),
                        new KeyValuePair<string, string>("refreshToken", RefreshToken),
             }
           );
        }

        var response = await HttpIfood.PostAsync("/authentication/v1.0/oauth/token", formDataToGetTheToken);
        var result = await response.Content.ReadFromJsonAsync<InformacoesDoTokenRetornadaPeloIfoodDto>();

        if (result is not null && result.AccessToken is not null && result.RefreshToken is not null)
        {

            if (!Refresh)
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
            else
            {
                ClsEmpresaIfood? empresa = await _nestApiService.RetornaEmpresaIfood(TokenNestAPi, IdEmpresa);
                if (empresa is not null)
                {
                    empresa.AccessTokenIfood = result.AccessToken;
                    empresa.RefreshTokenIfood = result.RefreshToken;
                    empresa.VenceTokenIfood = DateTime.Now.AddSeconds(result.ExpiresIn);

                    bool EditouEmpresa = await _nestApiService.EditarEAdicionarEmpresaIfood(empresa, TokenNestAPi, true, empresa);
                    if (EditouEmpresa)
                        return new ReturnApiRefatored<object>
                        {
                            Status = response.IsSuccessStatusCode ? "success" : "error",
                            Messages = new List<string> { "Token Atualizado com Sucesso!" },
                            Data = new Data<object> { ObjetoWhenWriting = empresa, Messages = new List<string> { "Token Atualizado com Sucesso!" } }
                        };
                }
            }
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
            if (Merchant.IntegraIfood)
                if (Merchant.EmpresasIfood.Count() > 0)
                {
                    var IfoodClient = _factory.CreateClient("ApiIfood");
                    foreach (var merchantsIfood in Merchant.EmpresasIfood)
                    {
                        if (!merchantsIfood.Ativo)
                            continue;

                        string AccessToken = merchantsIfood.AccessTokenIfood;
                        AdicionaTokenNaRequisicao(IfoodClient, AccessToken);

                        var PoolingResponse = await IfoodClient.GetAsync("/events/v1.0/events:polling");
                        if (PoolingResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            var result = await AutenticarEmpresa(null, TokenNestApi, true, merchantsIfood.RefreshTokenIfood, merchantsIfood.Id);
                            if (result.Status == "success" && result.Data.ObjetoWhenWriting is ClsEmpresaIfood empresaAtualizada)
                                AdicionaTokenNaRequisicao(IfoodClient, empresaAtualizada.AccessTokenIfood);

                            PoolingResponse = await IfoodClient.GetAsync("/events/v1.0/events:polling");
                            if (PoolingResponse.StatusCode == HttpStatusCode.Unauthorized)
                                continue;
                        }

                        if (PoolingResponse.IsSuccessStatusCode)
                        {
                            //Aqui você pode processar a resposta do pooling, por exemplo, lendo os pedidos e salvando no banco de dados
                            int statusCode = (int)PoolingResponse.StatusCode;
                            if (statusCode != 200)
                                continue;

                            List<PollingIfoodDto> Poolings = JsonSerializer.Deserialize<List<PollingIfoodDto>>(await PoolingResponse.Content.ReadAsStringAsync()) ?? new List<PollingIfoodDto>();
                            foreach (var P in Poolings)
                            {
                                switch (P.Code)
                                {
                                    case "PLC": //caso entre aqui é porque é um novo pedido     
                                        string MensagemDeTentativaDeAddPedido = await AdicionaPedidoAoSophos(P.OrderId, IfoodClient, TokenNestApi, Merchant, P);
                                        Messages.Add(MensagemDeTentativaDeAddPedido);
                                        continue;
                                    case "CFM":
                                        //Aqui qunado for aceito o pedido e vier a confimação
                                        continue;
                                    case "DSP":
                                        await MudaStatusPedidoDespachado(UpdateDto: new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, PedidoIdIntegracao = P.OrderId, TokenNestApi = TokenNestApi }, Polling: P);
                                        continue;
                                    case "CON":
                                        await MudaStatusPedidoConcluido(UpdateDto: new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, PedidoIdIntegracao = P.OrderId, TokenNestApi = TokenNestApi }, Polling: P);
                                        continue;
                                    case "CAN":
                                        await MudaStatusPedidoCancelado(UpdateDto: new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, PedidoIdIntegracao = P.OrderId, TokenNestApi = TokenNestApi }, Polling: P);
                                        continue;
                                    default:
                                        await MudaStatusComoINfosAdicionaisPedidoNaAPiPrincipal(UpdateDto: new UpdatePedidosDto { DestinoPedido = DestinoPedido.Sophos, PedidoIdIntegracao = P.OrderId, TokenNestApi = TokenNestApi }, Polling: P);
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

                    if (PollingsToAcknowledge.Count() > 0 && Merchant.EmitindoNfeProd)
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
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Conversão do pedido Ifood não válida. Endpoint: {Pooling}", "Pooling");
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Não Foi possivel ler o pedido do ifood" } };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Pooling do iFood cancelado. Endpoint: {Pooling}", "Pooling");
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "A requisição para leitura de pedidos demorou muito e foi cancelada." } };
        }
        catch (Exception ex)
        {
            _logger.LogError("Tipo real da exception: {Type}", ex.GetType().FullName);
            return new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao ler pedidos ifood" } };
        }
    }
    #endregion

    #region Funções de Ação dos Pedidos Do Ifood
    private async Task<PedidoIfoodDto?> GetPedidoIfood(string OrderId, HttpClient IfoodClient)
    {
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

    private async Task<bool> AceitaPedido(string PedidoId, HttpClient IfoodClient)
    {
        var ResponseConfirm = await IfoodClient.PostAsync($"order/v1.0/orders/{PedidoId}/confirm", null);
        return ResponseConfirm.IsSuccessStatusCode;
    }

    public async Task<bool> MudaStatusPedidoDespachado(UpdatePedidosDto UpdateDto, PollingIfoodDto? Polling = null)
    {
        HttpClient? HttpIntegracaoCliente = null;
        if (UpdateDto.DestinoPedido == DestinoPedido.Ifood)
        {
            HttpIntegracaoCliente = _factory.CreateClient("ApiIfood");
            AdicionaTokenNaRequisicao(HttpIntegracaoCliente, UpdateDto.MerchantIfood.AccessTokenIfood);
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
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.TokenNestApi, UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                var response = await _nestApiService.UpdatePedidoDespachadoNaAPiPrincipalAsync(UpdateDto.TokenNestApi, PedidoSophos);

                if (Polling is not null)
                {
                    if (response)
                        PollingsToAcknowledge.Add(Polling);
                }
            }


        }



        return true;
    }

    public async Task<bool> MudaStatusPedidoConcluido(UpdatePedidosDto UpdateDto, PollingIfoodDto? Polling = null)
    {
        HttpClient? HttpIntegracaoCliente = null;

        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos && UpdateDto.TokenNestApi is not null)
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.TokenNestApi, UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                var response = await _nestApiService.UpdatePedidoConcluidodoNaAPiPrincipalAsync(UpdateDto.TokenNestApi, PedidoSophos);

                if (Polling is not null)
                {
                    if (response)
                        PollingsToAcknowledge.Add(Polling);
                }
            }


        }

        return true;
    }

    public async Task<bool> MudaStatusPedidoCancelado(UpdatePedidosDto UpdateDto, PollingIfoodDto? Polling = null)
    {
        HttpClient? HttpIntegracaoCliente = null;
        if (UpdateDto.DestinoPedido == DestinoPedido.Ifood)
        {
            HttpIntegracaoCliente = _factory.CreateClient("ApiIfood");
            AdicionaTokenNaRequisicao(HttpIntegracaoCliente, UpdateDto.MerchantIfood.AccessTokenIfood);
            string PedidoIdIfood = UpdateDto.Pedido?.IfoodID ?? UpdateDto.PedidoIdIntegracao;

        }

        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos && UpdateDto.TokenNestApi is not null)
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.TokenNestApi, UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                var response = await _nestApiService.UpdatePedidoCanceladodoNaAPiPrincipalAsync(UpdateDto.TokenNestApi, PedidoSophos);

                if (Polling is not null)
                {
                    if (response)
                        PollingsToAcknowledge.Add(Polling);
                }
            }


        }



        return true;
    }
    public async Task<bool> MudaStatusComoINfosAdicionaisPedidoNaAPiPrincipal(UpdatePedidosDto UpdateDto, PollingIfoodDto Polling)
    {
        HttpClient? HttpIntegracaoCliente = null;
        if (UpdateDto.DestinoPedido == DestinoPedido.Sophos && UpdateDto.TokenNestApi is not null)
        {
            ClsPedido? PedidoSophos = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(UpdateDto.TokenNestApi, UpdateDto.PedidoIdIntegracao);
            if (PedidoSophos is not null)
            {
                string infoAdicional = RetornaStatusCompletoAtualizado(Polling.Code);

                var response = await _nestApiService.UpdatePedidoInfosAdicionaisOuStatusoNaAPiPrincipalAsync(UpdateDto.TokenNestApi, PedidoSophos, new UpdatePedidoInfosAdicionaisDto { InfoAdicionalOuStatus = infoAdicional });

                if (Polling is not null)
                {
                    PollingsToAcknowledge.Add(Polling);
                }
            }


        }

        return true;
    }
    #endregion

    #region Funções de Conversão de Pedido Ifood para Pedido do SOPHOS
    private async Task<ClsPedido?> ConvertePedidoDoIfoodParaPedidoSophos(PedidoIfoodDto PedidoIfood, string TokenNestApi, ClsMerchant MerchantSophos)
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

        PedidoSophos.Itens = await RetornaItensSophos(PedidoIfood.Items, TokenNestApi);

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

    private async Task<List<ItensPedido>> RetornaItensSophos(List<ItemIfoodDto> ItensIfood, string TokenNestApi)
    {
        //Fazer integração depois para buscar o produto no banco de dados do sophos e preencher o Id do produto e o Id do preço, por enquanto vai ser só a descrição mesmo

        var ItensSophos = new List<ItensPedido>();

        foreach (var item in ItensIfood)
        {
            var ResultadoConversaoCodPdv = EditaCodigoPdvIfoodParaPadroSophos(item);

            ClsProduto? ProdutoSophos = await _nestApiService.RetornaProdutoEncontrado(ResultadoConversaoCodPdv.Codigo, TokenNestApi);
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
                ClsComplemento? ComplementoSophosEncontrado = await _nestApiService.RetornaComplementoEncontrado(complemento.ExternalCode, TokenNestApi);

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

    private async Task<string> AdicionaPedidoAoSophos(string OrderId, HttpClient IfoodClient, string TokenNestApi, ClsMerchant Merchant, PollingIfoodDto P)
    {
        PedidoIfoodDto? PedidoIFood = await GetPedidoIfood(OrderId, IfoodClient);
        if (PedidoIFood is null)
        {
            return $"Falha ao obter os detalhes do pedido {OrderId} para o merchant {Merchant.NomeFantasia}";

        }

        ClsPedido? PedidoSophos = await ConvertePedidoDoIfoodParaPedidoSophos(PedidoIFood, TokenNestApi, Merchant);
        if (PedidoSophos is null)
        {
            return $"Falha ao converter o pedido {OrderId} para o formato do Sophos para o merchant {Merchant.NomeFantasia}";
        }

        bool AdicionouOPedido = await _nestApiService.CriarPedidoSophos(TokenNestApi, PedidoSophos);
        if (AdicionouOPedido)
        {
            if (Merchant.AceitaPedidoAutDeIntegracoes && Merchant.EmitindoNfeProd) //Aqui serve para podermos integrar com a loja do cliente mas não aceitar os pedidos pra ele, apenas visualizar 
            {
                bool AceitouPedido = await AceitaPedido(PedidoIFood.Id, IfoodClient);
                if (AceitouPedido)
                    PollingsToAcknowledge.Add(P);

            }
            return $"Pedido {OrderId} processado e adicionado com sucesso para o merchant {Merchant.NomeFantasia}";
        }
        else
        {
            return $"Falha ao adicionar o pedido {P.OrderId} para o merchant {Merchant.NomeFantasia}";
        }
    }


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
        switch (legendaTamnho)
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
            case "DDCR":
                return "Informe o código de confirmação na entrega do pedido";
            case "DDCS":
                return "O código de confirmação de entrega foi validado com sucesso";
            case "DPCS":
                return "O código de confirmação de coleta foi validado com sucesso";
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
                    PedidoReferente = await _nestApiService.GetPedidoPeloIntegracaoIdAsync(TokenNestApi, IdPedidoIfood);
                    if(PedidoReferente is null)
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
            AdicionaTokenNaRequisicao(IfoodClient, EmpresaIfood.AccessTokenIfood);

            var Response = await IfoodClient.GetAsync($"order/v1.0/orders/{PedidoReferente?.IfoodID ?? IdPedidoIfood}/cancellationReasons");
            List<ClsCancelationReasons>? CancelationReasons = await Response.Content.ReadFromJsonAsync<List<ClsCancelationReasons>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if(CancelationReasons is null)
                return new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Erro ao obter os motivos de cancelamento do ifood" } };


            return new ReturnApiRefatored<ClsCancelationReasons> { Status = "success", Messages = new List<string> { "Motivos De cancelamento encontrado com sucesso!"}, Data = new Data<ClsCancelationReasons> { ListWhenWriting = CancelationReasons, Messages = new List<string> { "Motivos De cancelamento encontrado com sucesso!" } } };
        }
        catch (Exception ex)
        {
            return new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Erro ao obter os motivos de cancelamento do ifood" } };
        }


    }
    #endregion
}


