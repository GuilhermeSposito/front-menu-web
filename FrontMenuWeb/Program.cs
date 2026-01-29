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
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("Api"));

builder.Services.AddScoped(sp =>
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return clientFactory.CreateClient("ApiAutorizada");
});

builder.Services.AddHttpClient("ApiAutorizada",(sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

void ConfigureApiFiscalSoophosClient(IHttpClientBuilder builder)
{
    builder.ConfigureHttpClient((sp,client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;

        client.BaseAddress = new Uri(settings.BaseUrlAPiFiscal);
    }).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
}

void ConfigureSyslogicaClient(IHttpClientBuilder builder)
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


ConfigureSyslogicaClient(builder.Services.AddHttpClient<GrupoServices>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<ProdutoService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<AliquotaService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<MerchantServices>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<PessoasService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<ContasService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<CategoriasService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<MetodosDePagMerchantService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<FormasDeRecebimentoService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<LancamentoFinanceiroService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<ComplementosServices>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<FuncionariosService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<MesasServices>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<PedidosService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<CaixaEPagamentosService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<EntregasMachineService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<DistanciasService>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<EntregasService>());
ConfigureApiFiscalSoophosClient(builder.Services.AddHttpClient<NfService>());


builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

builder.Services.AddScoped<IErrorBoundaryLogger, GlobalErrorHandler>();

var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
builder.Logging.SetMinimumLevel(LogLevel.Warning);

await builder.Build().RunAsync();

