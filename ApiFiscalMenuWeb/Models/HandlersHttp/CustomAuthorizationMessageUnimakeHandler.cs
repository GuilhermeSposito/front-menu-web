using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.DataProtection;
using System.Net;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.HandlersHttp;

public class CustomAuthorizationMessageUnimakeHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _factory;

    public CustomAuthorizationMessageUnimakeHandler(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = Environment.GetEnvironmentVariable("TOKEN_MESSAGE_BROKER");
        if (token is null)
        {
            var AuthMessageBrokerClient = _factory.CreateClient("ApiMessageBrokerUnimakeAuth");
            var responseAuthToken = await AuthMessageBrokerClient.PostAsJsonAsync("api/auth", new { appId = "086f8e04b6834581ba375f6ca947589b", secret = "ff9165403d9a4d318b5eef2267cce964" }, cancellationToken);
            var tokenAuthResponse = await responseAuthToken.Content.ReadFromJsonAsync<TokenMessageBrokerResponse>(cancellationToken: cancellationToken);

            if (tokenAuthResponse is not null)
            {
                Environment.SetEnvironmentVariable("TOKEN_MESSAGE_BROKER", tokenAuthResponse.Token, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("REFRESH_TOKEN_MESSAGE_BROKER", tokenAuthResponse.RefreshToken, EnvironmentVariableTarget.Process);
                token = tokenAuthResponse.Token;
            }
        }

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        // não entra em loop no refresh
        if (request.RequestUri!.AbsolutePath.Contains("/auth/refresh"))
            return response;

        var refreshClient = _factory.CreateClient("ApiMessageBrokerUnimakeAuth");
        Console.WriteLine("Entrou no refresh");

        var responseRefresh = await refreshClient.PostAsJsonAsync("api/auth", new { appId = "086f8e04b6834581ba375f6ca947589b", secret = "ff9165403d9a4d318b5eef2267cce964" }, cancellationToken);
        var tokenResponse = await responseRefresh.Content.ReadFromJsonAsync<TokenMessageBrokerResponse>(cancellationToken: cancellationToken);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            return response;

        Environment.SetEnvironmentVariable("TOKEN_MESSAGE_BROKER", tokenResponse.Token, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("REFRESH_TOKEN_MESSAGE_BROKER", tokenResponse.RefreshToken, EnvironmentVariableTarget.Process);

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var newRequest = await CloneHttpRequestMessage(request);

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

public class TokenMessageBrokerResponse
{
    [JsonPropertyName("expiration")] public float Expiration { get; set; }
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

}