using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.Components.Modais.ModaisDeCadastros.EmpresaIfood;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Integracoes;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Produtos;
using FrontMenuWeb.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiFiscalMenuWeb.Services;

public class NestApiServices
{
    #region Props
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _configuration;

    public NestApiServices(IHttpClientFactory factory, IConfiguration configuration)
    {
        _factory = factory;
        _configuration = configuration;
    }
    #endregion

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

    public async Task<ClsMerchant?> GetMerchantFromNestApiPublic(string IdMerchant)
    {
        try
        {
            var client = _factory.CreateClient("ApiAutorizada");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync($"merchants/details/public/{IdMerchant}", cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("A requisição para 'merchants/details' excedeu o tempo limite.");
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
                return null;

            var wrapper = JsonSerializer.Deserialize<ReturnApiRefatored<ClsMerchant>>(content);

            return wrapper?.Data?.Objeto;
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


    public async Task<ClsEmpresaIfood?> RetornaEmpresaIfood(string? TokenNestApi, int IdEmpresa)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");

        if (TokenNestApi is not null)
            AdicionaTokenNaRequisicao(client, TokenNestApi);

        var EmpresaServiceNest = new EmpresaIfoodService(client, _factory);
        var empresa = await EmpresaServiceNest.GetEmpresaIntegradaAsync(IdEmpresa);
        return empresa;
    }

    public async Task<ClsEmpresaIfood?> RetornaEmpresaIfoodPeloMerchantId(string IdEmpresa)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        var EmpresaServiceNest = new EmpresaIfoodService(client, _factory);
        var empresa = await EmpresaServiceNest.GetEmpresaIntegradaPeloMerchantIdAsync(IdEmpresa);
        return empresa;
    }

    public async Task<bool> CriarPedidoSophos(ClsMerchant Merchant, ClsPedido NovoPedido)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        var PedidoServiceNest = new PedidosService(client, _configuration);

        var RetornoDoCreate = await PedidoServiceNest.CreatePedidoPublicAsync(NovoPedido, Merchant, cts.Token);
        if (RetornoDoCreate.Status == "error")
        {
            Console.WriteLine($"[CriarPedidoSophos] Erro ao criar pedido. Merchant: {Merchant?.Id} | Mensagens: {string.Join(", ", RetornoDoCreate.Messages)} | Payload: {JsonSerializer.Serialize(NovoPedido)}");
            return false;
        }

        return RetornoDoCreate.Status == "success" ? true : false;
    }

    public async Task<ClsProduto?> RetornaProdutoEncontrado(string? CodigoPdv, string MerchantSophosId)
    {
        if (string.IsNullOrEmpty(CodigoPdv))
            return null;

        HttpClient client = _factory.CreateClient("ApiAutorizada");

        var ProdutoServiceNest = new ProdutoService(client);
        ClsProduto? prod = await ProdutoServiceNest.GetProdutoPorCodigoInternoAsync(MerchantSophosId, CodigoPdv);

        return prod;
    }

    public async Task<ClsComplemento?> RetornaComplementoEncontrado(string? CodigoPdv, string MerchantSophosId)
    {
        if (string.IsNullOrEmpty(CodigoPdv))
            return null;

        HttpClient client = _factory.CreateClient("ApiAutorizada");
        var ProdutoServiceNest = new ComplementosServices(client);

        bool CodigoPdvEUmNumeroValido = int.TryParse(CodigoPdv, out int codigoPdvNumerico);
        if (!CodigoPdvEUmNumeroValido)
        {
            return null;
        }

        var complemento = await ProdutoServiceNest.GetComplementoComMerchantId(codigoPdvNumerico, MerchantSophosId);
        return complemento;
    }


    public async Task<ClsPedido?> GetPedidoPeloIntegracaoIdAsync(string integracaoId)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.GetPedidoByIntegracaoId(integracaoId);

        return response;
    }
    public async Task<bool> UpdatePedidoDespachadoNaAPiPrincipalAsync(string? TokenNest, string? MerchantId, ClsPedido Pedido)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        if (!string.IsNullOrEmpty(TokenNest))
            AdicionaTokenNaRequisicao(client, TokenNest);

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.UpdatePedidoDespachadoEProntoPublic(Pedido, MerchantId);

        return true;
    }

    public async Task<bool> UpdatePedidoConcluidodoNaAPiPrincipalAsync(string? TokenNest, string? MerchantId, ClsPedido Pedido)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        if (!string.IsNullOrEmpty(TokenNest))
            AdicionaTokenNaRequisicao(client, TokenNest);

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.UpdatePedidoFinalizadoPublic(Pedido, MerchantSophosId: MerchantId);

        return true;
    }

    public async Task<bool> UpdatePedidoPreparandoNaAPiPrincipalAsync(string? TokenNest, string MerchantId, ClsPedido Pedido)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.UpdatePedidoPreparando(Pedido, MerchantId);

        return true;
    }

    public async Task<bool> UpdatePedidoCanceladodoNaAPiPrincipalAsync(ClsPedido Pedido)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.CancelarPedido(Pedido);

        return true;
    }

    public async Task<bool> UpdatePedidoInfosAdicionaisOuStatusoNaAPiPrincipalAsync(string MerchantSophosId, ClsPedido Pedido, UpdatePedidoInfosAdicionaisDto UpdateDto)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.UpdatePedidoInfosAdicionaisOuStatus(UpdateDto, Pedido, MerchantSophosId);

        return true;
    }

    public async Task<ClsMerchant?> GetMerchantByAnotaAiTokenAsync(string token)
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var response = await client.GetAsync($"merchants/by-anota-ai-token/{Uri.EscapeDataString(token)}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[NestApiServices] Merchant não encontrado para token AnotaAi. Status: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ReturnApiRefatored<ClsMerchant>>(content);
            return wrapper?.Data?.Objeto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NestApiServices] Erro ao buscar merchant por token AnotaAi: {ex.Message}");
            return null;
        }
    }

    public async Task<MerchantByInstanceDto?> GetMerchantByInstanceNameAsync(string instanceName)
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var response = await client.GetAsync($"merchants/by-instance/{Uri.EscapeDataString(instanceName)}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[NestApiServices] Merchant não encontrado para instanceName '{instanceName}'. Status: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MerchantByInstanceDto>(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NestApiServices] Erro ao buscar merchant por instanceName: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SalvarWhatsAppMensagemAsync(CriarWhatsAppMensagemDto dto)
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var response = await client.PostAsJsonAsync("whatsapp-mensagens", dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Console.WriteLine($"[NestApiServices] Mensagem '{dto.MessageId}' já registrada, ignorando duplicata.");
                return true;
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[NestApiServices] Falha ao salvar mensagem WhatsApp. Status: {response.StatusCode} | Body: {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NestApiServices] Erro ao salvar mensagem WhatsApp: {ex.Message}");
            return false;
        }
    }

    // ── Delmatch ─────────────────────────────────────────────────────────────

    public async Task<List<ApiFiscalMenuWeb.Models.Dtos.DelmatchEmpresaDto>> RetornaEmpresasDelmatchParaPolling()
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var response = await client.GetAsync("empresas-delmatch/all-for-polling");

            if (!response.IsSuccessStatusCode)
                return new();

            var result = await response.Content.ReadFromJsonAsync<ApiFiscalMenuWeb.Models.Dtos.DelmatchEmpresasPollingResponseDto>();
            return result?.Data?.Empresas ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NestApiServices] Erro ao buscar empresas Delmatch para polling: {ex.Message}");
            return new();
        }
    }

    public async Task<ApiFiscalMenuWeb.Models.Dtos.DelmatchEmpresaDto?> RetornaEmpresaDelmatchPeloId(int id)
    {
        try
        {
            var client = _factory.CreateClient("ApiAutorizada");
            var response = await client.GetAsync($"empresas-delmatch/by-id/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ApiFiscalMenuWeb.Models.Dtos.DelmatchEmpresaByIdResponseDto>();
            return result?.Data?.Empresa;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NestApiServices] Erro ao buscar empresa Delmatch {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Fire-and-forget: dispara a verificação/renovação de tokens Delmatch sem aguardar.
    /// </summary>
    public void SolicitaVerificacaoDeTokens()
    {
        var client = _factory.CreateClient("ApiNestPublica");
        _ = client.PostAsync("empresas-delmatch/verificar-tokens", null);
    }

    // ── CCM ──────────────────────────────────────────────────────────────────

    public async Task<List<ClsMerchant>> RetornaMerchantsComIntegracaoCcm()
    {
        try
        {
            var client = _factory.CreateClient("ApiNestPublica");
            var response = await client.GetAsync("merchants/integracao-ccm/todos");

            if (!response.IsSuccessStatusCode)
                return new();

            var merchants = await response.Content.ReadFromJsonAsync<List<ClsMerchant>>();
            return merchants ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NestApiServices] Erro ao buscar merchants com integração CCM: {ex.Message}");
            return new();
        }
    }
}
