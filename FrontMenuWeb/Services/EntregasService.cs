using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.EntregaMachine;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Raios;
using MudBlazor;
using Newtonsoft.Json;
using System.Net.Http.Json;
using YamlDotNet.Core.Tokens;

namespace FrontMenuWeb.Services;

public class EntregasService
{
    private HttpClient _http;
    public EntregasService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RaioDeEntrega>> GetRaiosDeEntregaAsync()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>("api-entregas/raio");
        return response?.Data.Lista ?? new List<RaioDeEntrega>();
    }

    public async Task<ReturnApiRefatored<RaioDeEntrega>> CreateRaio(RaioDeEntrega raio)
    {
        var response = await _http.PostAsJsonAsync($"api-entregas/raio", raio);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>() ?? new ReturnApiRefatored<RaioDeEntrega>() { Status = "error", Messages = new List<string> { "Erro ao Criar Raio de Entrega" } };
    }

    public async Task<ReturnApiRefatored<RaioDeEntrega>> UpdateRaio(RaioDeEntrega raio)
    {
        var response = await _http.PatchAsJsonAsync($"api-entregas/raio/{raio.Id}", raio);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>() ?? new ReturnApiRefatored<RaioDeEntrega>() { Status = "error", Messages = new List<string> { "Erro ao Atualizar Raio de Entrega" } };
    }

    public async Task<ReturnApiRefatored<RaioDeEntrega>> DeleteRaio(RaioDeEntrega raio)
    {
        var response = await _http.DeleteAsync($"api-entregas/raio/{raio.Id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<RaioDeEntrega>>() ?? new ReturnApiRefatored<RaioDeEntrega>() { Status = "error", Messages = new List<string> { "Erro ao Atualizar Raio de Entrega" } };
    }
}

public class MachineService
{

    private HttpClient _http;
    public MachineService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Dictionary<string, Severity>> EnviaPedidoParaOMapa(EmpresaMachine EmpresaMachine, ClsPedido PedidoQueVaiSeREnviado, AppState EstadoAtualDoApp)
    {
        try
        {
            EnderecoMerchant? EnderecoMerchant = EstadoAtualDoApp.MerchantLogado?.EnderecosMerchant?.FirstOrDefault();
            if (EnderecoMerchant is null)
                throw new Exception("Nenhum Endereço para esse estabelecimento foi encontrado, cadastre-o para continuar");

            var MinutosParaAdicionar = EstadoAtualDoApp.MerchantLogado?.TempoDeEntregaEMmin ?? 30;

            var EnderecoDeOrigem = new EnderecoDeOrigemDto
            {
                EnderecoFormatadoOrigem = EnderecoMerchant.EnderecoFormatado,
                BairroDeOrigem = EnderecoMerchant.Bairro,
                ComplementoDeOrigem = "",
                ReferenciaDeOrigem = "",
            };

            var EnderecoDeDestino = new EnderecoDeDestinoDto
            {
                IdDeReferenciaExterna = $"{PedidoQueVaiSeREnviado.DisplayId} - SOPHOS",
                EnderecoFormatadoDestino = PedidoQueVaiSeREnviado.Endereco?.EnderecoFormatado,
                BairroDestino = PedidoQueVaiSeREnviado.Endereco?.Bairro,
                ComplementoDestino = PedidoQueVaiSeREnviado.Endereco?.Complemento,
                ReferenciaDestino = PedidoQueVaiSeREnviado.Endereco?.Referencia,
                CidadeDestino = PedidoQueVaiSeREnviado.Endereco?.Cidade,
                EstadoDestino = PedidoQueVaiSeREnviado.Endereco?.Estado,
                NomeClienteDestino = PedidoQueVaiSeREnviado.Cliente?.Nome,
                TelefoneClienteDestino = PedidoQueVaiSeREnviado.Cliente?.Telefone
            };

            var Solicitacao = new SolicitacaoParaSerEnviadaDto
            {
                DataField = DateTime.Now.AddMinutes(MinutosParaAdicionar),
                EnderecoDeOrigem = EnderecoDeOrigem,
                EnderecoDeDestino = EnderecoDeDestino,
                FormaDePagamento = EmpresaMachine.TipoPagamento,
                retorno = false
            };


            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", EmpresaMachine.TokenApiEntrega);
            var response = await _http.PostAsJsonAsync("pedidos-machine/cadastrar-pedido", Solicitacao);

            var responseContent = await response.Content.ReadAsStringAsync();

            RetornoApiSophosEntregaResponse? respostaApi = JsonConvert.DeserializeObject<RetornoApiSophosEntregaResponse>(responseContent);

            if(respostaApi is not null)
            {
               if(respostaApi.Status == "success")
                    return new Dictionary<string, Severity> { { "Pedido enviado para o mapa de entregas com sucesso!", Severity.Success } };

                else
                    return new Dictionary<string, Severity> { { $"Erro ao enviar pedido para o mapa: {respostaApi.Messages}", Severity.Error } };
            }
            else
            {
                return new Dictionary<string, Severity> { { $"Erro ao enviar pedido para o mapa: Resposta da API não pôde ser processada", Severity.Error } };
            }
        }
        catch (Exception ex)
        {
            return new Dictionary<string, Severity> { { $"Erro ao enviar pedido para o mapa: Resposta da API não pôde ser processada", Severity.Error } };
        }
    }
}

