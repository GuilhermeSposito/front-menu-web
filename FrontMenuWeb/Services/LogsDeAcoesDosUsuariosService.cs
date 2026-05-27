using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Logs;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class LogsDeAcoesDosUsuariosService
{
    private readonly HttpClient _httpClient;

    public LogsDeAcoesDosUsuariosService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReturnApiRefatored<ClsLogAcaoUsuario>> AdminGetAllLogsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ReturnApiRefatored<ClsLogAcaoUsuario>>("logs-de-acoes-do-usuario/all");
        return response ?? new ReturnApiRefatored<ClsLogAcaoUsuario>();
    }
}
