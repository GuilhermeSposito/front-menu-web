using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Caixa;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class SuprimentosService
{
    private readonly HttpClient _http;

    public SuprimentosService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ReturnApiRefatored<ClsSuprimento>> GetSuprimentosAsync(int caixaId, string? status = null)
    {
        var queryParams = new List<string> { $"caixaId={caixaId}" };

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");

        var url = $"suprimentos?{string.Join("&", queryParams)}";

        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsSuprimento>>(url);
        return response ?? new ReturnApiRefatored<ClsSuprimento> { Status = "error", Messages = ["Erro ao buscar suprimentos"] };
    }

    public async Task<ReturnApiRefatored<ClsSuprimento>> CreateSuprimentoAsync(CreateSuprimentoDto dto)
    {
        var response = await _http.PostAsJsonAsync("suprimentos", dto);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSuprimento>>();
        return retorno ?? new ReturnApiRefatored<ClsSuprimento> { Status = "error", Messages = ["Erro ao registrar suprimento"] };
    }

    public async Task<ReturnApiRefatored<ClsSuprimento>> CancelarSuprimentoAsync(int id, CancelarSuprimentoDto dto)
    {
        var response = await _http.PatchAsJsonAsync($"suprimentos/{id}/cancelar", dto);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSuprimento>>();
        return retorno ?? new ReturnApiRefatored<ClsSuprimento> { Status = "error", Messages = ["Erro ao cancelar suprimento"] };
    }

    public async Task<PaginatedResponse<ClsSuprimento>> GetAllSuprimentosPaginatedAsync(int page = 1, int limit = 10, string? status = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var queryParams = new List<string> { $"page={page}", $"limit={limit}" };

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");

        if (dataInicio.HasValue)
            queryParams.Add($"dataInicio={dataInicio.Value.Date:yyyy-MM-dd}T00:00:00");

        if (dataFim.HasValue)
            queryParams.Add($"dataFim={dataFim.Value.Date:yyyy-MM-dd}T23:59:59");

        var url = $"suprimentos/all?{string.Join("&", queryParams)}";

        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsSuprimento>>(url);
        return response ?? new PaginatedResponse<ClsSuprimento>();
    }
}
