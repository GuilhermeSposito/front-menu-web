using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Caixa;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services;

public class SangriasService
{
    private readonly HttpClient _http;

    public SangriasService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ReturnApiRefatored<ClsSangria>> GetSangriasAsync(int caixaId, string? status = null)
    {
        var queryParams = new List<string> { $"caixaId={caixaId}" };

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");

        var url = $"sangrias?{string.Join("&", queryParams)}";

        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsSangria>>(url);
        return response ?? new ReturnApiRefatored<ClsSangria> { Status = "error", Messages = ["Erro ao buscar sangrias"] };
    }

    public async Task<PaginatedResponse<ClsSangria>> GetAllSangriasPaginatedAsync(int page = 1, int limit = 10, string? status = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var queryParams = new List<string> { $"page={page}", $"limit={limit}" };

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");

        if (dataInicio.HasValue)
            queryParams.Add($"dataInicio={dataInicio.Value.Date:yyyy-MM-dd}T00:00:00");

        if (dataFim.HasValue)
            queryParams.Add($"dataFim={dataFim.Value.Date:yyyy-MM-dd}T23:59:59");

        var url = $"sangrias/all?{string.Join("&", queryParams)}";

        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsSangria>>(url);
        return response ?? new PaginatedResponse<ClsSangria>();
    }

    public async Task<ReturnApiRefatored<ClsSangria>> CreateSangriaAsync(CreateSangriaDto dto)
    {
        var response = await _http.PostAsJsonAsync("sangrias", dto);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSangria>>();
        return retorno ?? new ReturnApiRefatored<ClsSangria> { Status = "error", Messages = ["Erro ao registrar sangria"] };
    }

    public async Task<ReturnApiRefatored<ClsSangria>> CancelarSangriaAsync(int id, CancelarSangriaDto dto)
    {
        var response = await _http.PatchAsJsonAsync($"sangrias/{id}/cancelar", dto);
        var retorno = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSangria>>();
        return retorno ?? new ReturnApiRefatored<ClsSangria> { Status = "error", Messages = ["Erro ao cancelar sangria"] };
    }

    public async Task<AuthorizationResponse> ValidateAuthorizeAsync(string email, string senha, string permissionName)
    {
        var response = await _http.PostAsJsonAsync("merchants/authorize-action", new { email, senha, action = permissionName });
        var retorno = await response.Content.ReadFromJsonAsync<AuthorizationResponse>();
        return retorno ?? new AuthorizationResponse { Allowed = false, Message = ["Erro ao validar autorização"] };
    }
}

public class AuthorizationResponse
{
    [JsonPropertyName("allowed")] public bool Allowed { get; set; }
    [JsonPropertyName("message")] public string[] Message { get; set; } = Array.Empty<string>();
}
