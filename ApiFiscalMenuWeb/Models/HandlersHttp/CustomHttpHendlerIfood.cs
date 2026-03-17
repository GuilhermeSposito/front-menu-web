using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using System.Net;

namespace ApiFiscalMenuWeb.Models.HandlersHttp;

public class CustomHttpHendlerIfood : DelegatingHandler
{
    private readonly IHttpClientFactory _factory;

    public CustomHttpHendlerIfood(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            string? token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");
            if (token is null)
            {
                bool Autenticou = await AutenticarEmpresa();
                if (Autenticou)
                    token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;


            var refreshClient = _factory.CreateClient("ApiIfood");
            await AutenticarEmpresa();
            token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");


            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var newRequest = await CloneHttpRequestMessage(request);

            return await base.SendAsync(newRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Erro ao autenticar com o iFood") };
        }
   
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

    public async Task<bool> AutenticarEmpresa()
    {
        try
        {
            var HttpIfood = _factory.CreateClient("ApiIfood");
            FormUrlEncodedContent formDataToGetTheToken = new FormUrlEncodedContent(new[]
            {
              new KeyValuePair<string, string>("grantType", "client_credentials"),
              new KeyValuePair<string, string>("clientId", "20bd3527-0599-4762-a773-b167dad2a9c8"),
              new KeyValuePair<string, string>("clientSecret", "4kyv4yt3b2cczztrdfihr8pihblgptoa9a5pw9ldmeq7tidz90nauhp2009opffjoh33ay1uy60unq3gw1vm8u72dm91ols7fry"),
        });

            var response = await HttpIfood.PostAsync("/authentication/v1.0/oauth/token", formDataToGetTheToken);

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS: {response.StatusCode}");
                Console.WriteLine($"BODY: {raw}");

                Console.WriteLine("Erro ao autenticar no iFood");
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<InformacoesDoTokenRetornadaPeloIfoodDto>();

            if (result is not null)
            {
                Environment.SetEnvironmentVariable("TOKEN_IFOOD_REQS", result.AccessToken, EnvironmentVariableTarget.Process);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao pegar token ifood");
            return false;
        }
    
    }
}
