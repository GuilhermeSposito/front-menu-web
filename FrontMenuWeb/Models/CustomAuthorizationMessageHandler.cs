
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SocketIOClient.Transport.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrontMenuWeb.Models;
public class CustomAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _configuration;


    public CustomAuthorizationMessageHandler(ILocalStorageService localStorage, IHttpClientFactory factory, IConfiguration configuration)
    {
        _localStorage = localStorage;
        _factory = factory;
        _configuration = configuration;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("x-api-key"))
        {
            var apiKey = _configuration["ApiKeyNest"];
            request.Headers.Add("x-api-key", apiKey);
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var key = Encoding.UTF8.GetBytes(_configuration["HMAC_SECRET"]!);
        var message = Encoding.UTF8.GetBytes(timestamp);
        using var hmac = new HMACSHA256(key);
        var hash = Convert.ToHexString(hmac.ComputeHash(message)).ToLower();
        request.Headers.Add("x-timestamp", timestamp);
        request.Headers.Add("x-hash", hash);

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
        {
            var RequestLogout = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
            RequestLogout.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var ResponseLogou = await refreshClient.SendAsync(RequestLogout, cancellationToken);

            return response;
        }

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
