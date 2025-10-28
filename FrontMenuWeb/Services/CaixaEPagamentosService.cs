﻿using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using SocketIO.Core;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontMenuWeb.Services;

public class CaixaEPagamentosService
{
    public HttpClient _HttpClient { get; set; }
    public CaixaEPagamentosService(HttpClient http)
    {
        _HttpClient = http;
    }
    public async Task<ReturnApiRefatored<ClsPedido>> VerificaSeHaCaixaAberto(int? FuncionarioId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "caixas/aberto");
        if (FuncionarioId.HasValue)
        {
            request.Headers.Add("funcionario_id", FuncionarioId.Value.ToString());
        }

        var response = await _HttpClient.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        var retorno = JsonSerializer.Deserialize<ReturnApiRefatored<ClsPedido>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return retorno ?? new ReturnApiRefatored<ClsPedido>
        {
            Status = "error",
            Messages = ["Erro ao buscar caixas abertos"]
        };
    }

}
