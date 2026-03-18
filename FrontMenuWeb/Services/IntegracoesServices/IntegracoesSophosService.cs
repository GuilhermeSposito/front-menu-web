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

    public async Task<ReturnApiRefatored<ClsCancelationReasons>> CancelationReasonsIfood(ClsPedido Pedido)
    {
        if (string.IsNullOrEmpty(Pedido.IfoodID))
            return new ReturnApiRefatored<ClsCancelationReasons>
            {
                Status = "error",
                Messages = new List<string> { "Não Foi possivel encontrar o identificador do pedido ifood referente." },
            };

        var Retorno = await _httpClient.GetAsync($"integracoes/ifood/cancelation-reasons?IdPedidoIfood={Pedido.IfoodID}");
        var RetornoDeserializado = await Retorno.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsCancelationReasons>>();

        return RetornoDeserializado ?? new ReturnApiRefatored<ClsCancelationReasons> { Status = "error", Messages = new List<string> { "Erro ao obter motivos para cancelamento." } };
    }

    public async Task<ReturnApiRefatored<object>> CancelationIfood(CancelationIfoodObjectDto Dto)
    {
        if (string.IsNullOrEmpty(Dto.Pedido?.IfoodID))
            return new ReturnApiRefatored<object>
            {
                Status = "error",
                Messages = new List<string> { "Não Foi possivel encontrar o identificador do pedido ifood referente." },
            };

        var Retorno = await _httpClient.PostAsJsonAsync($"integracoes/ifood/cancelation", Dto);
        var RetornoDeserializado = await Retorno.Content.ReadFromJsonAsync<ReturnApiRefatored<object>>();

        return RetornoDeserializado ?? new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Erro ao obter motivos para cancelamento." } };
    }

    public async Task<ReturnApiRefatored<object>> AceitaPedido(string IdPedidoIntegrado)
    {
        if (string.IsNullOrEmpty(IdPedidoIntegrado))
            return new ReturnApiRefatored<object>
            {
                Status = "error",
                Messages = new List<string> { "Não Foi possivel encontrar o identificador do pedido ifood referente." },
            };

        var Retorno = await _httpClient.PostAsync($"integracoes/ifood/accepted/{IdPedidoIntegrado}", null);

        return new ReturnApiRefatored<object> { Status = Retorno.IsSuccessStatusCode ? "success" : "error", Messages = new List<string> { Retorno.IsSuccessStatusCode ? "Pedido Aceito com sucesso" : "Erro ao aceitar pedido" } };

    }


}
