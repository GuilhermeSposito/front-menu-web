using ApiFiscalMenuWeb.Services.Integracoes;
using System.Net;

namespace ApiFiscalMenuWeb.Models.HandlersHttp;

public class CustomSophosHttpHendler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {

            if (!request.Headers.Contains("x-api-key"))
            {
                request.Headers.Add("x-api-key", "c1a9f3e7b5d2c8a6f0e4b1d9a7c3f6e8b2d5a9c1f7e3b6d4a8c0e2f9a1b7c3d5");
            }

            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Erro ao autenticar com o Sophos API") };
        }

    }


}
