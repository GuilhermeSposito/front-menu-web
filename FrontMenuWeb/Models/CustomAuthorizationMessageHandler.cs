
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SocketIOClient.Transport.Http;
using System.Net;
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

    /*protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
    }*/

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        // não entra em loop no refresh
        if (request.RequestUri!.AbsolutePath.Contains("/auth/refresh"))
            return response;

        var refreshClient = _factory.CreateClient("ApiRefresh");

        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
        refreshRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var refreshResponse = await refreshClient.SendAsync(refreshRequest, cancellationToken);

        if (!refreshResponse.IsSuccessStatusCode)
            return response;

        // clonar e reenviar request original
        var newRequest = await CloneHttpRequestMessage(request);
        newRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return await base.SendAsync(newRequest, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessage(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;

            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

}
