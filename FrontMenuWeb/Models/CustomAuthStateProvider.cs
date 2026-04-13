using Blazored.LocalStorage;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using YamlDotNet.Core.Tokens;

namespace FrontMenuWeb.Models;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly AppState _appState;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient, AppState appState)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _appState = appState;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {      
        try
        {
            var ValidarOToken = await _httpClient.GetAsync("merchants/session-info");

            if (!ValidarOToken.IsSuccessStatusCode)
            {
             
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            else
            {
                var merchant = await ValidarOToken.Content.ReadFromJsonAsync<ClsMerchant>();
                _appState.MerchantLogado = merchant ?? new ClsMerchant();
                _appState.IsFuncionario = merchant?.FuncionarioLogado != null;

                var merchantJson = JsonSerializer.Serialize(merchant);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, merchant.NomeFantasia),
                    new Claim(ClaimTypes.Email, merchant.Email),
                    new Claim(ClaimTypes.Name, merchant.NomeFantasia ?? merchant.RazaoSocial),
                    new Claim("merchant_id", merchant.Id),
                    new Claim("razao_social", merchant.RazaoSocial),
                    new Claim("nome_fantasia", merchant.NomeFantasia ?? ""),
                    new Claim("imagem_logo", merchant.ImagemLogo ?? ""),
                    new Claim("ativo", merchant.Ativo.ToString()),
                    new Claim("Merchant", merchantJson),
                    new Claim("emitindo_nfe", merchant.EmitindoNfeProd.ToString())
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");



                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }

        }
        catch (Exception ex)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
      
    }

 
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
