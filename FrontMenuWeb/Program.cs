using FrontMenuWeb;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using FrontMenuWeb.Models;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

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

builder.Services.AddMudServices();
await builder.Build().RunAsync();
