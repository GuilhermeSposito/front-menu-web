
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

    public CustomAuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response =  await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
           /* var refreshResponse = await _httpClient.PostAsync(
                "auth/refresh", null, cancellationToken);

            if (!refreshResponse.IsSuccessStatusCode)
            {
                await _localStorage.RemoveItemAsync("authToken");
                _navigation.NavigateTo("/login");
                return response;
            }

            var auth = await refreshResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

            await _localStorage.SetItemAsync("authToken", auth.Token);

            // refaz a request original
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.Token);

            response = await base.SendAsync(request, cancellationToken);*/
        }

        return response;
    }
}


/*var token = await _localStorage.GetItemAsync<string>("authToken");
if (!string.IsNullOrEmpty(token))
{
    //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
}*/