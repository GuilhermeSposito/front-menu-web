using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace FrontMenuWeb.Models;

public class CustomAuthorizationMessageHandlerApiEntregas : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public CustomAuthorizationMessageHandlerApiEntregas(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        var token = await _localStorage.GetItemAsync<string>("authEntregaToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}