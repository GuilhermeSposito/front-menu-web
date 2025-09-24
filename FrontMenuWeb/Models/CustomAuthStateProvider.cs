using System.Security.Claims;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace FrontMenuWeb.Models;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var ValidarOToken = await _httpClient.GetAsync("merchants/details");

        if (!ValidarOToken.IsSuccessStatusCode)
        {
            // Se o token não for válido, remova-o do armazenamento local e retorne um estado de autenticação vazio
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null; // Limpa o cabeçalho de autorização
            this.NotifyAuthenticationStateChanged();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        else
        {
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

    }

    // ✅ Este é o método que você precisa chamar no logout/login
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    claims.Add(new Claim(kvp.Key, item.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }
        }

        return claims;
    }

    /* private byte[] ParseBase64WithoutPadding(string base64)
     {
         switch (base64.Length % 4)
         {
             case 2: base64 += "=="; break;
             case 3: base64 += "="; break;
         }

         return Convert.FromBase64String(base64);
     }*/

    private byte[] ParseBase64WithoutPadding(string base64Url)
    {
        // Converte Base64URL para Base64 padrão
        string s = base64Url.Replace('-', '+').Replace('_', '/');

        // Adiciona padding se necessário
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }

        return Convert.FromBase64String(s);
    }
}
