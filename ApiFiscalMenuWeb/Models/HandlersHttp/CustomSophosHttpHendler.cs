using ApiFiscalMenuWeb.Services.Integracoes;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ApiFiscalMenuWeb.Models.HandlersHttp;

public class CustomSophosHttpHendler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public CustomSophosHttpHendler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {

            if (!request.Headers.Contains("x-api-key"))
            {
                request.Headers.Add("x-api-key", _configuration["ApiKeyNest"]);
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var key = Encoding.UTF8.GetBytes(_configuration["HMAC_SECRET"]!);
            var message = Encoding.UTF8.GetBytes(timestamp);
            using var hmac = new HMACSHA256(key);
            var hash = Convert.ToHexString(hmac.ComputeHash(message)).ToLower();
            request.Headers.Add("x-timestamp", timestamp);
            request.Headers.Add("x-hash", hash);

            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Erro ao autenticar com o Sophos API") };
        }

    }


}
