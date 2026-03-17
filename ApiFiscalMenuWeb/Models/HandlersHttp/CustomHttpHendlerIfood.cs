using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using OneOf.Types;
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
            Environment.SetEnvironmentVariable("TOKEN_IFOOD_REQS", "eyJraWQiOiJlZGI4NWY2Mi00ZWY5LTExZTktODY0Ny1kNjYzYmQ4NzNkOTMiLCJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiJ9.eyJzdWIiOiIyMGJkMzUyNy0wNTk5LTQ3NjItYTc3My1iMTY3ZGFkMmE5YzgiLCJvd25lcl9uYW1lIjoic29waG9zLWFwbGljYXRpdm9zIiwiaXNzIjoiaUZvb2QiLCJjbGllbnRfaWQiOiIyMGJkMzUyNy0wNTk5LTQ3NjItYTc3My1iMTY3ZGFkMmE5YzgiLCJhdWQiOlsiaXRlbSIsImZpbmFuY2lhbCIsImNhdGFsb2ciLCJsb2dpc3RpY3MiLCJtZXJjaGFudCIsInBpY2tpbmciLCJvYXV0aC1zZXJ2ZXIiLCJzaGlwcGluZyIsInJldmlldyIsImdyb2NlcmllcyIsImV2ZW50cyIsInByb21vdGlvbiIsIm9yZGVyIl0sImFwcF9uYW1lIjoic29waG9zLWFwbGljYXRpdm9zLWUtdGVjbm9sb2dpYS1sdGRhLXRlc3RlLWMiLCJzY29wZSI6WyJpdGVtIiwic2hpcHBpbmciLCJjYXRhbG9nIiwicmV2aWV3IiwibG9naXN0aWNzIiwibWVyY2hhbnQiLCJncm9jZXJpZXMiLCJwaWNraW5nIiwiZXZlbnRzIiwiY29uY2lsaWF0b3IiLCJwcm9tb3Rpb24iLCJvcmRlciJdLCJ0dmVyIjoidjIiLCJtZXJjaGFudF9zY29wZSI6WyI0MDU0ZWYzOS0xMzYzLTQxMzQtYTVmMi02ZjQzYmI1NGNhYjY6c2hpcHBpbmciLCI0MDU0ZWYzOS0xMzYzLTQxMzQtYTVmMi02ZjQzYmI1NGNhYjY6Y29uY2lsaWF0b3IiLCI0MDU0ZWYzOS0xMzYzLTQxMzQtYTVmMi02ZjQzYmI1NGNhYjY6aXRlbSIsIjQwNTRlZjM5LTEzNjMtNDEzNC1hNWYyLTZmNDNiYjU0Y2FiNjpjYXRhbG9nIiwiNDA1NGVmMzktMTM2My00MTM0LWE1ZjItNmY0M2JiNTRjYWI2OmxvZ2lzdGljcyIsIjQwNTRlZjM5LTEzNjMtNDEzNC1hNWYyLTZmNDNiYjU0Y2FiNjpldmVudHMiLCI0MDU0ZWYzOS0xMzYzLTQxMzQtYTVmMi02ZjQzYmI1NGNhYjY6cHJvbW90aW9uIiwiNDA1NGVmMzktMTM2My00MTM0LWE1ZjItNmY0M2JiNTRjYWI2Om9yZGVyIiwiNDA1NGVmMzktMTM2My00MTM0LWE1ZjItNmY0M2JiNTRjYWI2OnJldmlldyIsIjQwNTRlZjM5LTEzNjMtNDEzNC1hNWYyLTZmNDNiYjU0Y2FiNjpncm9jZXJpZXMiLCI0MDU0ZWYzOS0xMzYzLTQxMzQtYTVmMi02ZjQzYmI1NGNhYjY6cGlja2luZyIsIjQwNTRlZjM5LTEzNjMtNDEzNC1hNWYyLTZmNDNiYjU0Y2FiNjptZXJjaGFudCJdLCJleHAiOjE3NzM3OTUxMTIsImlhdCI6MTc3Mzc3MzUxMiwianRpIjoiMjBiZDM1MjctMDU5OS00NzYyLWE3NzMtYjE2N2RhZDJhOWM4IiwibWVyY2hhbnRfc2NvcGVkIjp0cnVlfQ.UAlO-RKpf8tw08AzX9WszyQ3InCBkWS9Y3CeYVtFUKBTnJgYytkdzBcUpljqEAT5EMM570t9BLkvSborpOQXBNdjl0WaNNPDDkJrZeuLrINde_tT8pvX-_FMt46fOYqceH5KJA0DDWsMFztknjBHIXsr6oqUFs-fqf0LgYGHVGU", EnvironmentVariableTarget.Process);

            string? token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");
           /* if (token is null)
            {
                bool Autenticou = await AutenticarEmpresa();
                if (Autenticou)
                    token = Environment.GetEnvironmentVariable("TOKEN_IFOOD_REQS");
            }*/

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;


            var refreshClient = _factory.CreateClient("ApiIfood");
           // await AutenticarEmpresa();
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
