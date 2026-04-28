using ApiFiscalMenuWeb.Services.Integracoes;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace ApiFiscalMenuWeb.Models.HandlersHttp;

public class CustomMetaWSHttpHendler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public CustomMetaWSHttpHendler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            string TokenMeta = _configuration["MetaApiToken"] ?? "";

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenMeta);

            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Erro ao autenticar com o Sophos API") };
        }

    }


}
