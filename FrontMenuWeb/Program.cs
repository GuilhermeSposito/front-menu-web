using FrontMenuWeb;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using FrontMenuWeb.Models;
using FrontMenuWeb.Services;
using System.Globalization;
using MudBlazor.Extensions;
using FrontMenuWeb.Models.Produtos;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped<GrupoServices>();
builder.Services.AddScoped<ProdutoService>();

builder.Services.AddScoped(sp =>
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return clientFactory.CreateClient("ApiAutorizada");
});


builder.Services.AddHttpClient("ApiAutorizada", client =>
{
    client.BaseAddress = new Uri("https://syslogicadev.com/api/v1/");
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
void ConfigureSyslogicaClient(IHttpClientBuilder builder)
{
    builder.ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://syslogicadev.com/api/v1/");
    }).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
}

ConfigureSyslogicaClient(builder.Services.AddHttpClient<GrupoServices>());
ConfigureSyslogicaClient(builder.Services.AddHttpClient<ProdutoService>());


builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
