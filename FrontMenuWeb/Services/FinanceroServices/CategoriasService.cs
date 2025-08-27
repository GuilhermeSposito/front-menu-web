using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using System.Net.Http.Json;

namespace FrontMenuWeb.Services.FinanceroServices;

public class CategoriasService
{
    public HttpClient _HttpClient { get; set; }
    public CategoriasService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsCategoria>?> GetCategoriasAsync()
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsCategoria>>("financeiro/categorias");
        return response?.Data.Lista ?? new List<ClsCategoria>();
    }

    public async Task<ClsCategoria?> GetCategoriaAsync(int IdDaCategoria)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsCategoria>>($"financeiro/categorias/{IdDaCategoria}");
        return response?.Data.Objeto ?? new ClsCategoria();
    }

    public async Task<ReturnApiRefatored<ClsCategoria>> AdicionarCategoriaAsync(ClsCategoria categoria)
    {
        var response = await _HttpClient.PostAsJsonAsync("financeiro/categorias", categoria);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsCategoria>>() ?? new ReturnApiRefatored<ClsCategoria>();
    }


    public async Task<ReturnApiRefatored<ClsCategoria>> UpdateCategoriaAsync(ClsCategoria categoria)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"financeiro/categorias/{categoria.Id}", categoria);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsCategoria>>() ?? new ReturnApiRefatored<ClsCategoria>();
    }

    public async Task<ReturnApiRefatored<ClsCategoria>> DeleteCategoriaAsync(ClsCategoria categoria)
    {
        var response = await _HttpClient.DeleteAsync($"financeiro/categorias/{categoria.Id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsCategoria>>() ?? new ReturnApiRefatored<ClsCategoria>();
    }

    //------------------------------------------Parte de Sub Categoria--------------------------------------------------//
    public async Task<ReturnApiRefatored<ClsSubCategoria>> AdicionarSubCategoriaAsync(ClsSubCategoria categoria)
    {
        var response = await _HttpClient.PostAsJsonAsync("financeiro/categorias/sub", categoria);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSubCategoria>>() ?? new ReturnApiRefatored<ClsSubCategoria>();
    }

    public async Task<List<ClsSubCategoria>> GetSubsCategoriasAsync(int SubCategoriaId)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsSubCategoria>>($"financeiro/categorias/subs/{SubCategoriaId}");
        return response?.Data.Lista ?? new List<ClsSubCategoria>();
    }
    public async Task<ClsSubCategoria> GetSubCategoriaAsync(int SubCategoriaId)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsSubCategoria>>($"financeiro/categorias/sub/{SubCategoriaId}");
        return response?.Data.Objeto ?? new ClsSubCategoria();
    }

    public async Task<ReturnApiRefatored<ClsSubCategoria>> UpdateSubCategoriaAsync(ClsSubCategoria categoria)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"financeiro/categorias/sub/{categoria.Id}", categoria);
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSubCategoria>>() ?? new ReturnApiRefatored<ClsSubCategoria>();
    }
    public async Task<ReturnApiRefatored<ClsSubCategoria>> DeleteSubCategoriaAsync(ClsSubCategoria subcategoria)
    {
        var response = await _HttpClient.DeleteAsync($"financeiro/categorias/sub/{subcategoria.Id}");
        return await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsSubCategoria>>() ?? new ReturnApiRefatored<ClsSubCategoria>();
    }

}
