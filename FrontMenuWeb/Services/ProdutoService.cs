using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services;

public class ProdutoService
{
    private HttpClient _http;
    public ProdutoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsProduto>> GetProdutosAsync()
    {
        List<ClsProduto> response = await _http.GetFromJsonAsync<List<ClsProduto>>("produtos") ?? new List<ClsProduto>();

        return response;
    }


}
