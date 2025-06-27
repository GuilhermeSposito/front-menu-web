using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using FrontMenuWeb.DTOS;

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
    
    public async Task<ClsProduto> GetProdutoAsync(string ProdutoID)
    {
        ClsProduto response = await _http.GetFromJsonAsync<ClsProduto>($"produtos/{ProdutoID}") ?? new ClsProduto();

        return response;
    }

    public async Task<HttpResponseMessage> AdicionaProdutoAsync(ClsProduto produto)
    {
        var response = await _http.PostAsJsonAsync<ClsProduto>($"produtos/create", produto);

        return response;
    }

    public async Task<HttpResponseMessage> EditaProduto(ClsProduto produto)
    {
        var response = await _http.PatchAsJsonAsync($"produtos/{produto.Id}", produto);
        return response;

    }  
    
    public async Task<HttpResponseMessage> EditaPrecoDoProduto(Preco preco)
    {
        var response = await _http.PatchAsJsonAsync($"produtos/preco/modificar/{preco.Id}", preco);
        return response;
    }    
    
    public async Task<HttpResponseMessage> DeletaProduto(ClsProduto produto)
    {
        var response = await _http.DeleteAsync($"produtos/{produto.Id}");
        return response;
    }   
    public async Task<HttpResponseMessage> DeletaPreco(Preco preco)
    {
        var response = await _http.DeleteAsync($"produtos/preco/deletar/{preco.Id}");
        return response;
    } 
    
    public async Task<HttpResponseMessage> AdicionaValorNoProduto(string idProduto, Preco preco)
    {
        AdicionarPrecoDto precoDto = new AdicionarPrecoDto()
        {
            DescricaoDoTamanho = preco.DescricaoDoTamanho,
            CustosDoInsumo = preco.CustosDoInsumo,
            CustoReal = preco.CustoReal,
            PrecoSujetido = preco.PrecoSujetido,
            PorcentagemDeLucro = preco.PorcentagemDeLucro,
            Valor = preco.Valor
        };

        var response = await _http.PostAsJsonAsync($"produtos/preco/adicionar/{idProduto}", precoDto);
        return response;
    }   
    
  
}
