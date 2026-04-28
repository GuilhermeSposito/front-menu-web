using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Roteirizacao;
using System.Drawing;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Unimake.MessageBroker.Primitives.Model.Messages;
using Unimake.MessageBroker.Primitives.Model.Notifications;
using Unimake.MessageBroker.Services;
using Unimake.Primitives.UDebug;
namespace ApiFiscalMenuWeb.Services;

public class MessageService
{
    private readonly IHttpClientFactory _factory;
    private readonly Unimake.MessageBroker.Services.MessageService _messageService;
    public MessageService(IHttpClientFactory factory)
    {
        _factory = factory;
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

    #region Função para envio de mensagem com APi Oficial do WhatsApp (Meta)
    public async Task SendMessageStatusOficialAsync(EnviaMsgDto enviaMsgDto, ClsMerchant Merchant)
    {
        HttpClient WSMetaClient = _factory.CreateClient("ApiOficialMetaWS");

        string IdDoMerchantMeta = Merchant.InstanceName ?? throw new BadHttpRequestException("InstanceName do Merchant não pode ser nulo.");
        string MensagemDeAtualizacaoDeStatus = Merchant.MenssagemDeDeliveryDespachado ?? throw new BadHttpRequestException("Mensagem de atualização de status não pode ser nula.");

        SendMessageDtoWS MessageDto = new SendMessageDtoWS
        {
            To = $"55{enviaMsgDto.Pedido.Cliente?.Telefone}",
            Type = TipoMensagem.Template,
            Template = new TemplateDto
            {
                Name = TemplatesName.StatusPedido,
                Language = new LanguageDto { Code = "pt_BR" },
                Components = new List<ComponentDto>()
                {
                    new ComponentDto
                    {
                        Type = ComponentType.Body,
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Type = "text", Text = enviaMsgDto.Pedido.Cliente?.Nome ?? "Cliente" },
                        }
                    },
                    new ComponentDto
                    {
                        Type = ComponentType.Body,
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Type = "text", Text = MensagemDeAtualizacaoDeStatus },
                        }
                    },
                    new ComponentDto
                    {
                        Type = ComponentType.Button,
                        Index = "0",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Type = "text", Text = Merchant.Id },
                        }
                    }
                }
            },
        };


        var PostMessage = await WSMetaClient.PostAsJsonAsync($"{IdDoMerchantMeta}/messages", enviaMsgDto.Pedido, CancellationToken.None);
        Console.WriteLine(PostMessage.StatusCode);
    }

    #endregion

}
