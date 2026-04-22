using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Pedidos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services.IntegracoesServices;

public class MessageWhatsAppService
{
    public HttpClient _httpClient { get; set; }
    public MessageWhatsAppService(HttpClient http)
    {
        _httpClient = http;
    }

    public async Task EnviarMensagem(ClsPedido Pedido, EtapasPedido etapa)
    {
        var EnvMensagemDto = new EnviaMsgDto
        {
            EtapaDoPedido = etapa,
            NumeroDoPedido = Pedido.DisplayId,
            Pedido = Pedido
        };

        var response = await _httpClient.PostAsJsonAsync("message/enviar-msg", EnvMensagemDto);

        if (response.IsSuccessStatusCode)
        {
            // Mensagem enviada com sucesso
            Console.WriteLine("Mensagem enviada com sucesso!");
        }
        else
        {
            // Ocorreu um erro ao enviar a mensagem
            Console.WriteLine($"Erro ao enviar mensagem: {response.StatusCode}");
        }
    }

    public async Task EnviarMensagemMotoboy(EnviaMsgMotoboyDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("message/enviar-msg-motoboy", dto);

        if (response.IsSuccessStatusCode)
            Console.WriteLine("Mensagem enviada ao motoboy com sucesso!");
        else
            Console.WriteLine($"Erro ao enviar mensagem ao motoboy: {response.StatusCode}");
    }
}
