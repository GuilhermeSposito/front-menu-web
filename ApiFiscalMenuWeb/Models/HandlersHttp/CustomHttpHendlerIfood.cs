using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services.Integracoes;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using OneOf.Types;
using System.Net;

namespace ApiFiscalMenuWeb.Models.HandlersHttp;

public class CustomHttpHendlerIfood : DelegatingHandler
{
    private readonly IHttpClientFactory _factory;
    private readonly IfoodServices _ifoodService;

    public CustomHttpHendlerIfood(IHttpClientFactory factory, IfoodServices ifoodServicee)
    {
        _factory = factory;
        _ifoodService = ifoodServicee;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            string? token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");
            if (token is null)
            {
                bool Autenticou = await _ifoodService.AutenticarEmpresa();
                if (Autenticou)
                    token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;


            var refreshClient = _factory.CreateClient("ApiIfood");
            await _ifoodService.AutenticarEmpresa();
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

  
}
