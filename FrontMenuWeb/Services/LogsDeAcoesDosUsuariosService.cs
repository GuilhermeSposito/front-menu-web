using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Logs;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class LogsDeAcoesDosUsuariosService
{
    private readonly HttpClient _httpClient;

    public LogsDeAcoesDosUsuariosService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedResponse<ClsLogAcaoUsuario>> AdminGetAllLogsAsync(
        int page = 1,
        int limit = 20,
        DateTime? dataInicio = null,
        TimeSpan? horaInicio = null,
        DateTime? dataFim = null,
        TimeSpan? horaFim = null,
        string? merchantId = null,
        string? descricao = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"limit={limit}"
        };

        if (dataInicio.HasValue)
        {
            var dtInicio = dataInicio.Value.Date;
            if (horaInicio.HasValue) dtInicio = dtInicio.Add(horaInicio.Value);
            queryParams.Add($"DataInicio={dtInicio:yyyy-MM-ddTHH:mm:ss}");
        }

        if (dataFim.HasValue)
        {
            var dtFim = dataFim.Value.Date;
            if (horaFim.HasValue) dtFim = dtFim.Add(horaFim.Value);
            else dtFim = dtFim.AddDays(1).AddTicks(-1);
            queryParams.Add($"DataFim={dtFim:yyyy-MM-ddTHH:mm:ss}");
        }

        if (!string.IsNullOrWhiteSpace(merchantId))
            queryParams.Add($"merchantId={Uri.EscapeDataString(merchantId)}");

        if (!string.IsNullOrWhiteSpace(descricao))
            queryParams.Add($"descricao={Uri.EscapeDataString(descricao)}");

        var url = "logs-de-acoes-do-usuario/all?" + string.Join("&", queryParams);
        var response = await _httpClient.GetFromJsonAsync<PaginatedResponse<ClsLogAcaoUsuario>>(url);
        return response ?? new PaginatedResponse<ClsLogAcaoUsuario>();
    }
}
