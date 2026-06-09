using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Antecipacoes;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services;

public class AntecipacoesService
{
    private readonly HttpClient _http;

    public AntecipacoesService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ReturnApiRefatored<ClsAntecipacao>> CriarAntecipacaoAsync(CriarAntecipacaoDto dto)
    {
        var response = await _http.PostAsJsonAsync("antecipacoes", dto);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsAntecipacao>>()
               ?? new ReturnApiRefatored<ClsAntecipacao> { Status = "error", Messages = ["Erro ao criar antecipação"] };
    }

    public async Task<ReturnApiRefatored<ClsAntecipacao>> AtualizarAntecipacaoAsync(int id, CriarAntecipacaoDto dto)
    {
        var response = await _http.PutAsJsonAsync($"antecipacoes/{id}", dto);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsAntecipacao>>()
               ?? new ReturnApiRefatored<ClsAntecipacao> { Status = "error", Messages = ["Erro ao atualizar antecipação"] };
    }

    public async Task<ReturnApiRefatored<ClsAntecipacao>> DeletarAntecipacaoAsync(int id)
    {
        var response = await _http.DeleteAsync($"antecipacoes/{id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsAntecipacao>>()
               ?? new ReturnApiRefatored<ClsAntecipacao> { Status = "error", Messages = ["Erro ao deletar antecipação"] };
    }

    public async Task<List<ClsAntecipacao>> GetAntecipacoesAbertasPorMesaAsync(int mesaId)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsAntecipacao>>($"antecipacoes/mesa/{mesaId}/abertas");
        return response?.Data.Lista ?? new List<ClsAntecipacao>();
    }
}

public class CriarAntecipacaoDto
{
    [JsonPropertyName("mesaId")] public int MesaId { get; set; }
    [JsonPropertyName("valorAntecipacao")] public float? ValorAntecipacao { get; set; }
    [JsonPropertyName("nomeClienteAntecipacao")] public string? NomeClienteAntecipacao { get; set; }
    [JsonPropertyName("formaDeRecebimentoId")] public int? FormaDeRecebimentoId { get; set; }
    [JsonPropertyName("fechado")] public bool Fechado { get; set; } = false;
}
