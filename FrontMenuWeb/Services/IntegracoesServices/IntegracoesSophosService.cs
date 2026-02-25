namespace FrontMenuWeb.Services.IntegracoesServices;

public class IntegracoesSophosService
{
    private readonly HttpClient _httpClient;
    public IntegracoesSophosService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PollingParaVisualizacaoDeNovosPedidos()
    {
        try
        {
            await _httpClient.GetAsync("/integracoes/polling");
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
     
    }
}
