using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Roteirizacao;
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
        sb.AppendLine($"Olá {dto.NomeMotoboy}, segue o roteiro das entregas:");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(dto.LinkGoogleMaps))
        {
            sb.AppendLine("Rota completa no Google Maps:");
            sb.AppendLine(dto.LinkGoogleMaps);
            sb.AppendLine();
        }

        for (int i = 0; i < dto.Paradas.Count; i++)
        {
            var parada = dto.Paradas[i];
            string endereco = MontaEnderecoParada(parada);
            sb.AppendLine($"Parada {i + 1}: {endereco}");

            if (parada.Lat.HasValue && parada.Lng.HasValue)
            {
                sb.AppendLine($"https://www.google.com/maps/search/?api=1&query={parada.Lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{parada.Lng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }

            if (!string.IsNullOrWhiteSpace(parada.Cliente))
                sb.AppendLine($"Cliente: {parada.Cliente}");

            if (!string.IsNullOrWhiteSpace(parada.Telefone))
                sb.AppendLine($"Telefone: {parada.Telefone}");

            sb.AppendLine();
        }

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
}
