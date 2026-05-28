using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Roteirizacao;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Unimake.MessageBroker.Primitives.Model.Messages;
namespace ApiFiscalMenuWeb.Services;

public class MessageService
{
    private readonly IHttpClientFactory _factory;
    private readonly NestApiServices _nestApiServices;
    private readonly WhatsAppOptInService _optInService;

    private const string MensagemAutomatica = "Olá! Recebemos sua mensagem. Em breve nossa equipe entrará em contato. 😊";

    private static readonly HashSet<string> PalavrasDeOptOut = new(StringComparer.OrdinalIgnoreCase)
        { "PARAR", "STOP", "SAIR", "CANCELAR", "DESCADASTRAR" };

    public MessageService(IHttpClientFactory factory, NestApiServices nestApiServices, WhatsAppOptInService optInService)
    {
        _factory = factory;
        _nestApiServices = nestApiServices;
        _optInService = optInService;
    }

    #region Funções de conexão com a API Nest
    public async Task<ClsMerchant?> GetMerchantFromNestApi(string token)
    {
        try
        {
            var client = _factory.CreateClient("ApiAutorizada");
            AdicionaTokenNaRequisicao(client, token);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

            var response = await client.GetAsync("merchants/details", cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[MessageService] Falha ao obter merchant. Status: {response.StatusCode} | Body: {content}");
                return null;
            }

            var merchant = JsonSerializer.Deserialize<ClsMerchant>(content);

            if (merchant is null)
                Console.WriteLine($"[MessageService] Desserialização retornou null. Body: {content}");

            return merchant;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Erro ao obter merchant: {ex.Message}");
            return null;
        }
    }

    private void AdicionaTokenNaRequisicao(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    #endregion

    #region Funções de envio de mensagens com a unimake
    public async Task SendMessageAsync(EnviaMsgDto enviaMsgDto, string TokenDaApiNest)
    {
        try
        {
            ClsMerchant? Merchant = await GetMerchantFromNestApi(TokenDaApiNest);
            if (Merchant is null) return;

            if (Merchant.IntegraApiOficialWS)
            {
                await SendMessageStatusOficialAsync(enviaMsgDto, Merchant);
                return;
            }

            string Mensagem = string.Empty;
            if (enviaMsgDto.EtapaDoPedido == EtapasPedido.PREPARANDO)
            {
                var HttpSophosClient = _factory.CreateClient("ApiAutorizada");
                AdicionaTokenNaRequisicao(HttpSophosClient, TokenDaApiNest);
                var GetMensagem = await HttpSophosClient.PostAsJsonAsync($"gemini/preparando?nomeestabelecimento={Merchant.NomeFantasia}", enviaMsgDto.Pedido, CancellationToken.None);

                if (!GetMensagem.IsSuccessStatusCode) return;
                Mensagem = await GetMensagem.Content.ReadAsStringAsync();
            }
            else if (enviaMsgDto.EtapaDoPedido == EtapasPedido.DESPACHADO)
            {
                Mensagem = enviaMsgDto.Pedido.TipoDePedido == "DELIVERY"
                    ? Merchant.MenssagemDeDeliveryDespachado ?? ""
                    : Merchant.MenssagemDeBalcaoPronto ?? "";
            }
            else if (enviaMsgDto.EtapaDoPedido == EtapasPedido.FINALIZADO)
            {
                Mensagem = enviaMsgDto.Pedido.TipoDePedido == "DELIVERY"
                    ? Merchant.MenssagemDeDeliveryFinalizado ?? ""
                    : Merchant.MenssagemDeBalcaoFinalizado ?? "";
            }
            else
            {
                return;
            }

            if (string.IsNullOrEmpty(Mensagem)) return;

            string? NomeDoCliente = enviaMsgDto.Pedido.Cliente?.Nome;
            string? NumeroDoCliente = enviaMsgDto.Pedido.Cliente?.Telefone;
            string? InstanceName = Merchant.InstanceName;

            if (string.IsNullOrEmpty(NomeDoCliente) || string.IsNullOrEmpty(NumeroDoCliente) || string.IsNullOrEmpty(InstanceName))
            {
                Console.WriteLine($"[MessageService] Dados insuficientes para enviar msg. Cliente: {NomeDoCliente}, Tel: {NumeroDoCliente}, Instance: {InstanceName}");
                return;
            }

            string NumeroDoClienteFormatado = $"55{NumeroDoCliente}";
            string MensagemFormatada = Mensagem.Replace("{NomeDoCliente}", NomeDoCliente).Replace("{NumeroDoPedido}", enviaMsgDto.NumeroDoPedido);

            TextMessage Message = new TextMessage
            {
                InstanceName = InstanceName,
                Text = MensagemFormatada,
                To = NumeroDoClienteFormatado,
            };

            HttpClient client = _factory.CreateClient("ApiMessageBrokerUnimake");
            var response = await client.PostAsJsonAsync($"/umessenger/api/v1/Messages/Publish/{InstanceName}", Message);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("Mensagem enviada com sucesso!");
            else
                Console.WriteLine($"Falha ao enviar mensagem. Status Code: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Erro ao enviar mensagem WhatsApp: {ex.Message}");
        }
    }

    public async Task SendMessageMotoboyAsync(EnviaMsgMotoboyDto enviaMsgMotoboyDto, string TokenDaApiNest)
    {
        try
        {
            ClsMerchant? Merchant = await GetMerchantFromNestApi(TokenDaApiNest);
            if (Merchant is null) return;

            string? InstanceName = Merchant.InstanceName;
            string? TelefoneMotoboy = enviaMsgMotoboyDto.TelefoneMotoboy;

            if (string.IsNullOrEmpty(InstanceName) || string.IsNullOrEmpty(TelefoneMotoboy) || enviaMsgMotoboyDto.Paradas.Count == 0)
            {
                Console.WriteLine($"[MessageService] Dados insuficientes para enviar msg ao motoboy. Tel: {TelefoneMotoboy}, Instance: {InstanceName}, Paradas: {enviaMsgMotoboyDto.Paradas.Count}");
                return;
            }

            string Mensagem = MontaMensagemMotoboy(enviaMsgMotoboyDto);

            string NumeroFormatado = FormataNumeroWhatsApp(TelefoneMotoboy);

            TextMessage Message = new TextMessage
            {
                InstanceName = InstanceName,
                Text = Mensagem,
                To = NumeroFormatado,
                PreviewUrl = true,
            };

            HttpClient client = _factory.CreateClient("ApiMessageBrokerUnimake");
            var response = await client.PostAsJsonAsync($"/umessenger/api/v1/Messages/Publish/{InstanceName}", Message);

            if (response.IsSuccessStatusCode)
                Console.WriteLine($"Mensagem enviada ao motoboy {enviaMsgMotoboyDto.NomeMotoboy} com sucesso!");
            else
                Console.WriteLine($"Falha ao enviar mensagem ao motoboy. Status Code: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Erro ao enviar mensagem ao motoboy: {ex.Message}");
        }
    }

    private static string MontaMensagemMotoboy(EnviaMsgMotoboyDto dto)
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(dto.LinkGoogleMaps))
        {
            sb.AppendLine(dto.LinkGoogleMaps);
            sb.AppendLine();
        }

        sb.AppendLine($"🏍️ *Olá, {dto.NomeMotoboy}!*");
        sb.AppendLine($"Segue o roteiro com *{dto.Paradas.Count}* {(dto.Paradas.Count == 1 ? "entrega" : "entregas")} 👇");
        sb.AppendLine();
        sb.AppendLine("━━━━━━━━━━━━━━━");
        sb.AppendLine();

        for (int i = 0; i < dto.Paradas.Count; i++)
        {
            var parada = dto.Paradas[i];
            string endereco = MontaEnderecoParada(parada);

            sb.AppendLine($"📍 *Parada {i + 1}*");
            sb.AppendLine(endereco);

            if (parada.Lat.HasValue && parada.Lng.HasValue)
            {
                string lat = parada.Lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lng = parada.Lng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                sb.AppendLine($"🗺️ https://www.google.com/maps/search/?api=1&query={lat},{lng}");
            }

            if (!string.IsNullOrWhiteSpace(parada.Cliente))
                sb.AppendLine($"👤 {parada.Cliente}");

            if (!string.IsNullOrWhiteSpace(parada.Telefone))
                sb.AppendLine($"📞 {parada.Telefone}");

            if (parada.Valor > 0)
                sb.AppendLine($"💰 Pedido: {parada.Valor.ToString("C", new System.Globalization.CultureInfo("pt-BR"))}");

            if (!string.IsNullOrWhiteSpace(parada.FormaPagamento))
            {
                string statusPag = parada.PagamentoOnline ? "✅ Pago online" : "💵 A receber";
                sb.AppendLine($"{statusPag} • {parada.FormaPagamento}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("━━━━━━━━━━━━━━━");
        sb.AppendLine("Boas entregas! 🚀");

        return sb.ToString().TrimEnd();
    }

    private static string MontaEnderecoParada(PedidoParaRota parada)
    {
        if (!string.IsNullOrWhiteSpace(parada.Rua) || !string.IsNullOrWhiteSpace(parada.Numero))
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(parada.Rua)) partes.Add(parada.Rua);
            if (!string.IsNullOrWhiteSpace(parada.Numero)) partes.Add($"nº {parada.Numero}");
            if (!string.IsNullOrWhiteSpace(parada.Bairro)) partes.Add(parada.Bairro);
            if (!string.IsNullOrWhiteSpace(parada.Cidade)) partes.Add(parada.Cidade);
            return string.Join(", ", partes);
        }

        return parada.Endereco ?? string.Empty;
    }

    private static string FormataNumeroWhatsApp(string numero)
    {
        var digitos = new string(numero.Where(char.IsDigit).ToArray());
        if (digitos.StartsWith("55")) return digitos;
        return $"55{digitos}";
    }
    #endregion

    public async Task SendMessageMotoboyOficialAsync(EnviaMsgMotoboyDto dto, string TokenDaApiNest)
    {
        try
        {
            ClsMerchant? merchant = await GetMerchantFromNestApi(TokenDaApiNest);
            if (merchant is null) return;

            string phoneNumberId = merchant.InstanceName ?? throw new BadHttpRequestException("InstanceName do Merchant não pode ser nulo.");

            if (string.IsNullOrEmpty(dto.TelefoneMotoboy))
            {
                Console.WriteLine("[MessageService] Telefone do motoboy não informado.");
                return;
            }

            var messageDto = new SendMessageDtoWS
            {
                To   = FormataNumeroWhatsApp(dto.TelefoneMotoboy),
                Type = TipoMensagem.template,
                Template = new TemplateDto
                {
                    Name     = TemplatesName.mensagem_motoboy_rotas,
                    Language = new LanguageDto { Code = "pt_BR" },
                    Components =
                    [
                        new ComponentDto
                        {
                            Type = ComponentType.body,
                            Parameters =
                            [
                                new() { Type = "text", Text = dto.NomeMotoboy ?? "Motoboy" },
                                new() { Type = "text", Text = dto.LinkGoogleMaps ?? "" },
                            ]
                        }
                    ]
                }
            };

            HttpClient wsClient = _factory.CreateClient("ApiOficialMetaWS");
            var response = await wsClient.PostAsJsonAsync($"{phoneNumberId}/messages", messageDto);

            if (response.IsSuccessStatusCode)
                Console.WriteLine($"[MessageService] Mensagem oficial enviada ao motoboy {dto.NomeMotoboy} com sucesso!");
            else
                Console.WriteLine($"[MessageService] Falha ao enviar mensagem oficial ao motoboy. Status: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Erro ao enviar mensagem oficial ao motoboy: {ex.Message}");
        }
    }

    #region Função para envio de mensagem com APi Oficial do WhatsApp (Meta)
    public async Task SendMessageStatusOficialAsync(EnviaMsgDto enviaMsgDto, ClsMerchant Merchant)
    {
        var telefoneCliente = FormataNumeroWhatsApp(enviaMsgDto.Pedido.Cliente?.Telefone ?? "");
        var estaOptIn = await _optInService.EstaOptInAsync(Merchant.Id, telefoneCliente);
        if (!estaOptIn)
        {
            Console.WriteLine($"[MessageService] {telefoneCliente} não possui opt-in para merchant {Merchant.Id} — mensagem não enviada.");
            return;
        }

        HttpClient WSMetaClient = _factory.CreateClient("ApiOficialMetaWS");

        string IdDoMerchantMeta = Merchant.InstanceName ?? throw new BadHttpRequestException("InstanceName do Merchant não pode ser nulo.");
        string MensagemDeAtualizacaoDeStatus = await RetornaMensagemDeStatus(enviaMsgDto.EtapaDoPedido, Pedido: enviaMsgDto.Pedido, merchant: Merchant);

        SendMessageDtoWS MessageDto = new SendMessageDtoWS
        {
            To = FormataNumeroWhatsApp(enviaMsgDto.Pedido.Cliente?.Telefone ?? ""),
            Type = TipoMensagem.template,
            Template = new TemplateDto
            {
                Name = TemplatesName.status_pedido,
                Language = new LanguageDto { Code = "pt_BR" },
                Components = new List<ComponentDto>()
                {
                    new ComponentDto
                    {
                        Type = ComponentType.header,
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Type = "text", Text = enviaMsgDto.Pedido.Cliente?.Nome ?? "Cliente" },
                        }
                    },
                    new ComponentDto
                    {
                        Type = ComponentType.body,
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Type = "text", Text = MensagemDeAtualizacaoDeStatus },
                        }
                    },
                    new ComponentDto
                    {
                        Type = ComponentType.button,
                        SubType = "url",
                        Index = "0",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto
                            {
                                Type = "text",
                                Text = Merchant.Id
                            }
                        }
                    }
                }
            },
        };


        var PostMessage = await WSMetaClient.PostAsJsonAsync($"{IdDoMerchantMeta}/messages", MessageDto);
        Console.WriteLine(await PostMessage.Content.ReadAsStringAsync());
    }

    public async Task<string> RetornaMensagemDeStatus(EtapasPedido etapa, ClsPedido Pedido, ClsMerchant merchant)
    {
        try
        {
            string Mensagem = string.Empty;

            if (etapa == EtapasPedido.PREPARANDO)
            {
                var HttpSophosClient = _factory.CreateClient("ApiAutorizada");

                var GetMensagem = await HttpSophosClient.PostAsJsonAsync($"gemini/preparando?nomeestabelecimento={merchant.NomeFantasia}", Pedido, CancellationToken.None);

                if (!GetMensagem.IsSuccessStatusCode) return "";

                Mensagem = await GetMensagem.Content.ReadAsStringAsync();

                return Mensagem;
            }

            if (Pedido.TipoDePedido == "DELIVERY")
                return etapa switch
                {
                    EtapasPedido.DESPACHADO => merchant.MenssagemDeDeliveryDespachado ?? "",
                    EtapasPedido.FINALIZADO => merchant.MenssagemDeDeliveryFinalizado ?? "",
                    _ => ""
                };

            if (Pedido.TipoDePedido == "BALCÃO")
                return etapa switch
                {
                    EtapasPedido.DESPACHADO => merchant.MenssagemDeBalcaoPronto ?? "",
                    EtapasPedido.FINALIZADO => merchant.MenssagemDeBalcaoFinalizado ?? "",
                    _ => ""
                };

            return "";
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    #endregion

    #region Processamento de mensagens recebidas via Webhook

    public async Task ProcessarMensagemRecebidaAsync(WhatsAppWebhookDto dto)
    {
        try
        {
            var change = dto?.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault();
            var value = change?.Value;

            var mensagem = value?.Messages?.FirstOrDefault();
            var status = value?.Statuses?.FirstOrDefault();

            if (mensagem != null)
            {
                string? phoneNumberId = value?.Metadata?.PhoneNumberId;

                if (string.IsNullOrEmpty(phoneNumberId) || string.IsNullOrEmpty(mensagem.From) || string.IsNullOrEmpty(mensagem.Id))
                {
                    Console.WriteLine("[MessageService] Mensagem recebida com campos obrigatórios ausentes, ignorando.");
                    return;
                }

                var merchant = await _nestApiServices.GetMerchantByInstanceNameAsync(phoneNumberId);
                if (merchant is null)
                {
                    Console.WriteLine($"[MessageService] Nenhum merchant encontrado para phoneNumberId '{phoneNumberId}', mensagem não será salva.");
                    return;
                }

                await MarcarMensagemComoLidaAsync(phoneNumberId, mensagem.Id);

                // Verifica se cliente enviou palavra de opt-out
                var bodyRecebido = mensagem.Text?.Body?.Trim() ?? "";
                if (PalavrasDeOptOut.Contains(bodyRecebido))
                {
                    await _optInService.RegistrarOptOutAsync(merchant.Id, mensagem.From);
                    Console.WriteLine($"[MessageService] Opt-out registrado para {mensagem.From} via mensagem '{bodyRecebido}'.");
                    return;
                }

                var autoReplySent = await EnviarRespostaAutomaticaAsync(phoneNumberId, mensagem.From);

                var receivedAt = long.TryParse(mensagem.Timestamp, out var ts)
                    ? DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime
                    : DateTime.UtcNow;

                await _nestApiServices.SalvarWhatsAppMensagemAsync(new CriarWhatsAppMensagemDto
                {
                    MerchantId    = merchant.Id,
                    PhoneNumberId = phoneNumberId,
                    FromNumber    = mensagem.From,
                    MessageId     = mensagem.Id,
                    Type          = mensagem.Type ?? "unknown",
                    Body          = mensagem.Text?.Body,
                    ReceivedAt    = receivedAt.ToString("o"),
                    AutoReplySent = autoReplySent,
                });
            }

            if (status != null)
            {
                Console.WriteLine($"[MessageService] Status recebido — Id: {status.Id} | Status: {status.Status} | RecipientId: {status.RecipientId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Erro ao processar mensagem recebida: {ex.Message}");
        }
    }

    private async Task MarcarMensagemComoLidaAsync(string phoneNumberId, string messageId)
    {
        var client = _factory.CreateClient("ApiOficialMetaWS");
        var payload = new { messaging_product = "whatsapp", status = "read", message_id = messageId };
        var response = await client.PostAsJsonAsync($"{phoneNumberId}/messages", payload);
        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"[MessageService] Falha ao marcar mensagem como lida: {await response.Content.ReadAsStringAsync()}");
    }

    private async Task<bool> EnviarRespostaAutomaticaAsync(string phoneNumberId, string para)
    {
        var client = _factory.CreateClient("ApiOficialMetaWS");
        var payload = new SendMessageDtoWS
        {
            To   = para,
            Type = TipoMensagem.text,
            Text = new TextSimpleMessageDto { Body = MensagemAutomatica }
        };
        var response = await client.PostAsJsonAsync($"{phoneNumberId}/messages", payload);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[MessageService] Resposta automática enviada para {para}");
            return true;
        }

        Console.WriteLine($"[MessageService] Falha ao enviar resposta automática: {await response.Content.ReadAsStringAsync()}");
        return false;
    }

    #endregion

    #region Solicitação de deleção de dados (Facebook Data Deletion Callback)

    public DataDeletionResponseDto ProcessarSolicitacaoDeDelecaoDados(string signedRequest, string appSecret)
    {
        var confirmationCode = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var statusUrl = "https://sophos-erp.com.br/privacy-policy";

        var partes = signedRequest.Split('.');
        if (partes.Length != 2)
        {
            Console.WriteLine("[MessageService] Data Deletion: signed_request em formato inválido.");
            return new DataDeletionResponseDto { Url = statusUrl, ConfirmationCode = confirmationCode };
        }

        var encodedSig = partes[0];
        var encodedPayload = partes[1];

        if (!ValidarAssinaturaSignedRequest(encodedSig, encodedPayload, appSecret))
        {
            Console.WriteLine("[MessageService] Data Deletion: assinatura inválida.");
            return new DataDeletionResponseDto { Url = statusUrl, ConfirmationCode = confirmationCode };
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(encodedPayload));
            var payload = JsonSerializer.Deserialize<FacebookSignedPayloadDto>(payloadJson);
            Console.WriteLine($"[MessageService] Data Deletion solicitada para user_id: {payload?.UserId} em {DateTimeOffset.FromUnixTimeSeconds(payload?.IssuedAt ?? 0)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Data Deletion: erro ao decodificar payload: {ex.Message}");
        }

        return new DataDeletionResponseDto { Url = statusUrl, ConfirmationCode = confirmationCode };
    }

    private bool ValidarAssinaturaSignedRequest(string encodedSig, string encodedPayload, string appSecret)
    {
        try
        {
            var sigBytes = Base64UrlDecode(encodedSig);
            var payloadBytes = Encoding.UTF8.GetBytes(encodedPayload);
            var secretBytes = Encoding.UTF8.GetBytes(appSecret);

            using var hmac = new System.Security.Cryptography.HMACSHA256(secretBytes);
            var expectedBytes = hmac.ComputeHash(payloadBytes);

            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(sigBytes, expectedBytes);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var base64 = input.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }

    #endregion

}
