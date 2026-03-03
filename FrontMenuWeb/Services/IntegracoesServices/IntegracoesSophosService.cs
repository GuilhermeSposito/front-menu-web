using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services.IntegracoesServices;

public class IntegracoesSophosService
{
    private readonly HttpClient _httpClient;
    public IntegracoesSophosService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PollingParaVisualizacaoDeNovosPedidos()
    {
        try
        {
            await _httpClient.GetAsync("integracoes/polling");
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
    }

    public async Task<ReturnApiRefatored<ClsCancelationReasons>> CancelationReasonsIfood(ClsPedido Pedido)
    {
        if(string.IsNullOrEmpty(Pedido.IfoodID))
            return new ReturnApiRefatored<ClsCancelationReasons>
            {
                Status = "error",
                Messages = new List<string> { "Não Foi possivel encontrar o identificador do pedido ifood referente." },
            };

        var Retorno = await _httpClient.GetAsync($"integracoes/ifood/cancelation-reasons?IdPedidoIfood={Pedido.IfoodID}");
        var RetornoDeserializado = await Retorno.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsCancelationReasons>>();

        return RetornoDeserializado ?? new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Erro ao obter motivos para cancelamento."} };
    }

    public async Task<ReturnApiRefatored<object>> CancelationIfood(CancelationIfoodObjectDto Dto)
    {
        if(string.IsNullOrEmpty(Dto.Pedido?.IfoodID))
            return new ReturnApiRefatored<object>
            {
                Status = "error",
                Messages = new List<string> { "Não Foi possivel encontrar o identificador do pedido ifood referente." },
            };

        var Retorno = await _httpClient.PostAsJsonAsync($"integracoes/ifood/cancelation", Dto);
        var RetornoDeserializado = await Retorno.Content.ReadFromJsonAsync<ReturnApiRefatored<object>>();

        return RetornoDeserializado ?? new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao obter motivos para cancelamento."} };
    }


}
