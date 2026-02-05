
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SocketIOClient.Transport.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FrontMenuWeb.Models;
public class CustomAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IHttpClientFactory _factory;

    public CustomAuthorizationMessageHandler(ILocalStorageService localStorage, IHttpClientFactory factory)
    {
        _localStorage = localStorage;
        _factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response =  await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshClient = _factory.CreateClient("ApiRefresh");

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
            refreshRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var refreshResponse = await refreshClient.SendAsync(refreshRequest);
        }

        return response;
    }
}
