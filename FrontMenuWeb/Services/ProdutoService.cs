using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using Nextended.Core.Extensions;
using MudBlazor.Extensions.Components.ObjectEdit;

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

    public async Task<PaginatedResponse<ClsProduto>> GetProdutosPorPaginaAsync(int page, int pageSize, string? pesquisaNome, int? pesquisaDeGrupo)
    {
        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsProduto>>(
           $"produtos/pagination?page={page}&limit={pageSize}&descricao={pesquisaNome}&grupo={pesquisaDeGrupo}");

        return response!;
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


public class ComplementosServices
{
    private HttpClient _http;
    public ComplementosServices(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClsGrupoDeComplemento>> GetGruposDeComplementos()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsGrupoDeComplemento>>("complementos/grupo-de-complementos") ?? new ReturnApiRefatored<ClsGrupoDeComplemento>();

        return response.Data.Lista ?? new List<ClsGrupoDeComplemento>();
    }

    public async Task<ClsGrupoDeComplemento> GetGrupoDeComplementos(int idDoGrupo)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsGrupoDeComplemento>>($"complementos/grupo-de-complementos/{idDoGrupo}") ?? new ReturnApiRefatored<ClsGrupoDeComplemento>();
        return response.Data.Objeto ?? new ClsGrupoDeComplemento();
    }

    public async Task<ClsComplemento> GetComplemento(int IdDoComplemento)
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<ClsComplemento>>($"complementos/{IdDoComplemento}") ?? new ReturnApiRefatored<ClsComplemento>();

        return response.Data.Objeto ?? new ClsComplemento();
    }

    public async Task<PaginatedResponse<ClsComplemento>> GetComplementosPagineted(int page, int pageSize)
    {
        var response = await _http.GetFromJsonAsync<PaginatedResponse<ClsComplemento>>($"complementos?limit={pageSize}&page={page}") ?? new PaginatedResponse<ClsComplemento>();

        return response ?? new PaginatedResponse<ClsComplemento>();
    }

    public async Task<ReturnApiRefatored<ClsGrupoDeComplemento>> UpdateGrupoDeComplemento(ClsGrupoDeComplemento grupo)
    {
        var response = await _http.PatchAsJsonAsync($"complementos/grupo-de-complementos/{grupo.Id}", grupo);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsGrupoDeComplemento>>() ?? new ReturnApiRefatored<ClsGrupoDeComplemento>();
        return result;
    }
    public async Task<ReturnApiRefatored<ClsComplemento>> UpdateComplemento(ClsComplemento complemento)
    {
        complemento.GruposIds = complemento.Grupos.Select(x => x.Grupo.Id).ToList();

        var response = await _http.PatchAsJsonAsync($"complementos/{complemento.Id}", complemento);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsComplemento>>() ?? new ReturnApiRefatored<ClsComplemento>();
        return result;
    }
}