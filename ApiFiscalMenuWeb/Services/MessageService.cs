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

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync("merchants/details", cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("A requisição para 'merchants/details' excedeu o tempo limite.");
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
                return null;

            var merchant = JsonSerializer.Deserialize<ClsMerchant>(content);

            return merchant;
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException("A requisição para 'merchants/details' excedeu o tempo limite.");
        }
        catch (Exception ex)
        {
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
        ClsMerchant Merchant = await GetMerchantFromNestApi(TokenDaApiNest) ?? throw new Exception("Não foi possível obter os detalhes do comerciante.");

        string Mensagem = string.Empty;
        if (enviaMsgDto.EtapaDoPedido == EtapasPedido.DESPACHADO)
        {
            if (enviaMsgDto.Pedido.TipoDePedido == "DELIVERY")
            {
                Mensagem = Merchant.MenssagemDeDeliveryDespachado ?? "";
            }
            else
            {
                Mensagem = Merchant.MenssagemDeBalcaoPronto ?? "";
            }
        }
        else if (enviaMsgDto.EtapaDoPedido == EtapasPedido.FINALIZADO)
        {
            if (enviaMsgDto.Pedido.TipoDePedido == "DELIVERY")
            {
                Mensagem = Merchant.MenssagemDeDeliveryFinalizado ?? "";
            }
            else
            {
                Mensagem = Merchant.MenssagemDeBalcaoFinalizado ?? "";
            }
        }
        else
        {
            throw new Exception("Etapa do pedido não suportada para envio de mensagem.");
        }

        string NomeDoCliente = enviaMsgDto.Pedido.Cliente?.Nome ?? throw new Exception("Nome do cliente não encontrado");
        string NumeroDoCliente = enviaMsgDto.Pedido.Cliente?.Telefone ?? throw new Exception("Número do cliente não encontrado");
        string InstanceName = Merchant.InstanceName ?? throw new Exception("Não foi possível obter o instance name");
        string NumeroDoClienteFormatado = $"55{NumeroDoCliente}";
        string MensagemFormatada = Mensagem.Replace("{NomeDoCliente}", NomeDoCliente).Replace("{NumeroDoPedido}", enviaMsgDto.NumeroDoPedido);

        TextMessage Message = new TextMessage
        {
            InstanceName = InstanceName,
            Text = MensagemFormatada,
            To = NumeroDoClienteFormatado,
        };

        HttpClient client = _factory.CreateClient("ApiMessageBrokerUnimake");

        try
        {
            var response = await client.PostAsJsonAsync($"/api/v1/Messages/Publish/{InstanceName}", Message);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Mensagem enviada com sucesso!");
            }
            else
            {
                Console.WriteLine($"Falha ao enviar mensagem. Status Code: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");
        }


    }
}
