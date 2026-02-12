using Blazored.LocalStorage;
using FrontMenuWeb;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Produtos;
using FrontMenuWeb.Services;
using FrontMenuWeb.Services.FinanceroServices;
using FrontMenuWeb.Services.Fiscal;
using FrontMenuWeb.Services.ServicosDeTerceiros;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped<GrupoServices>();
builder.Services.AddScoped<GrupoServices>();
builder.Services.AddScoped<AliquotaService>();
builder.Services.AddScoped<MerchantServices>();
builder.Services.AddScoped<PessoasService>();
builder.Services.AddScoped<NfService>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<EntregasMachineService>();
builder.Services.AddScoped<EntregasService>();
builder.Services.AddScoped<DistanciasService>();
builder.Services.AddScoped<MachineService>();
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("Api"));

builder.Services.AddScoped(sp =>
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return clientFactory.CreateClient("ApiAutorizada");
});

builder.Services.AddHttpClient("ApiRefresh", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
});

builder.Services.AddHttpClient("ApiAutorizada",(sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

void ConfigureApiFiscalSophosClient(IHttpClientBuilder builder)
{
    builder.ConfigureHttpClient((sp,client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;

        client.BaseAddress = new Uri(settings.BaseUrlAPiFiscal);
    }).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
}

void ConfigureSophosApiWebClient(IHttpClientBuilder builder)
{
    builder.ConfigureHttpClient((sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
    })
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
}

builder.Services.AddHttpClient<CEPService>(client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/ws/");
});

builder.Services.AddHttpClient<CnpjPesquisaService>(client =>
{
    client.BaseAddress = new Uri("https://brasilapi.com.br/api/cnpj/v1/");
});

builder.Services.AddHttpClient<MachineService>(client =>
{
    client.BaseAddress = new Uri("https://sophos-erp.com.br/api-entregas-sophos/v1/");
});


ConfigureSophosApiWebClient(builder.Services.AddHttpClient<GrupoServices>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<ProdutoService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<AliquotaService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<MerchantServices>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<PessoasService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<ContasService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<CategoriasService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<MetodosDePagMerchantService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<FormasDeRecebimentoService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<LancamentoFinanceiroService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<ComplementosServices>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<FuncionariosService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<MesasServices>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<PedidosService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<CaixaEPagamentosService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<EntregasMachineService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<DistanciasService>());
ConfigureSophosApiWebClient(builder.Services.AddHttpClient<EntregasService>());
ConfigureApiFiscalSophosClient(builder.Services.AddHttpClient<NfService>());




builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

builder.Services.AddScoped<IErrorBoundaryLogger, GlobalErrorHandler>();

var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
builder.Logging.SetMinimumLevel(LogLevel.Warning);

await builder.Build().RunAsync();

