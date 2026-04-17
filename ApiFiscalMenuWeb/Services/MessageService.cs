using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using System.Net.Http.Headers;
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

    public async Task SendMessageAsync(EnviaMsgDto enviaMsgDto, string TokenDaApiNest)
    {
        try
        {
            ClsMerchant? Merchant = await GetMerchantFromNestApi(TokenDaApiNest);
            if (Merchant is null) return;

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
}
