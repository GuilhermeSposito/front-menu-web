using FrontMenuWeb.Models.FichaTecnica;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontMenuWeb.Services;

public class FichaTecnicaService
{
    private readonly HttpClient _http;

    public FichaTecnicaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsFichaTecnica>> BuscarFichasDoProduto(string produtoId)
    {
        return await _http.GetFromJsonAsync<List<ClsFichaTecnica>>($"ficha-tecnica/produto/{produtoId}")
               ?? new List<ClsFichaTecnica>();
    }

    public async Task<ClsFichaTecnica?> CriarFicha(string produtoId, string? precoProdutoId)
    {
        var body = new CreateFichaTecnicaRequest { ProdutoId = produtoId, PrecoProdutoId = precoProdutoId };
        var response = await _http.PostAsJsonAsync("ficha-tecnica", body);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ClsFichaTecnica>();
    }

    public async Task<ClsItemFichaTecnica?> AdicionarItem(int fichaId, string produtoInsumoId, float quantidade, string unidadeMedida)
    {
        var body = new CreateItemFichaTecnicaRequest
        {
            ProdutoInsumoId = produtoInsumoId,
            Quantidade = quantidade,
            UnidadeMedida = unidadeMedida,
        };
        var response = await _http.PostAsJsonAsync($"ficha-tecnica/{fichaId}/item", body);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ClsItemFichaTecnica>();
    }

    public async Task<bool> AtualizarItem(int itemId, float quantidade, string unidadeMedida)
    {
        var body = new UpdateItemFichaTecnicaRequest { Quantidade = quantidade, UnidadeMedida = unidadeMedida };
        var response = await _http.PatchAsJsonAsync($"ficha-tecnica/item/{itemId}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoverItem(int itemId)
    {
        var response = await _http.DeleteAsync($"ficha-tecnica/item/{itemId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletarFicha(int fichaId)
    {
        var response = await _http.DeleteAsync($"ficha-tecnica/{fichaId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ClsProduto>> BuscarInsumos(string? filtro = null)
    {
        var url = string.IsNullOrWhiteSpace(filtro)
            ? "ficha-tecnica/insumos"
            : $"ficha-tecnica/insumos?q={Uri.EscapeDataString(filtro)}";
        return await _http.GetFromJsonAsync<List<ClsProduto>>(url) ?? new List<ClsProduto>();
    }
}
