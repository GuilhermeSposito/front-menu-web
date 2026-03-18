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

    private void AdicionaTokenNaRequisicao(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ClsEmpresaIfood?> EditarEAdicionarEmpresaIfood(ClsEmpresaIfood empresa, string? TokenNestApi, string MerchantSophosId , bool Editando = false, ClsEmpresaIfood? EmpresaASerEditada = null)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");
        if (!string.IsNullOrEmpty(TokenNestApi))
            AdicionaTokenNaRequisicao(client, TokenNestApi);

        var EmpresaServiceNest = new EmpresaIfoodService(client, _factory);

        ReturnApiRefatored<ClsEmpresaIfood>? RetornoDoCreate = null;

        if (!Editando)
        {
            RetornoDoCreate = await EmpresaServiceNest.CreateEmpresaPublic(empresa, MerchantSophosId);
        }
        else
        {
            if (EmpresaASerEditada != null)
                RetornoDoCreate = await EmpresaServiceNest.UpdateEmpresa(EmpresaASerEditada);
        }

        if (RetornoDoCreate is null)
            return null;

        return RetornoDoCreate.Data.Objeto;
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
            Console.WriteLine(string.Join(",", RetornoDoCreate.Messages));
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

    public async Task<ClsComplemento?> RetornaComplementoEncontrado(string? CodigoPdv)
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

        var complemento = await ProdutoServiceNest.GetComplemento(codigoPdvNumerico);
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

    public async Task<bool> UpdatePedidoCanceladodoNaAPiPrincipalAsync(ClsPedido Pedido)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.CancelarPedido(Pedido);

        return true;
    }

    public async Task<bool> UpdatePedidoInfosAdicionaisOuStatusoNaAPiPrincipalAsync(string MerchantSophosId ,ClsPedido Pedido, UpdatePedidoInfosAdicionaisDto UpdateDto)
    {
        HttpClient client = _factory.CreateClient("ApiAutorizada");

        PedidosService PedidoServiceNest = new PedidosService(client, _configuration);
        var response = await PedidoServiceNest.UpdatePedidoInfosAdicionaisOuStatus(UpdateDto, Pedido, MerchantSophosId);

        return true;
    }
}
