using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services;

public class MerchantServices
{
    public HttpClient _HttpClient { get; set; }
    public MerchantServices(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<ClsMerchant> GetMerchantAsync()
    {
        var response = await _HttpClient.GetFromJsonAsync<ClsMerchant>("merchants/details");
        return response ?? new ClsMerchant();
    }

    public async Task<ReturnApiRefatored<ClsMerchant>> UpdateMerchantAsync(ClsMerchant merchant)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"merchants/update/{merchant.Id}", merchant);
        var updatedMerchant = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMerchant>>();
        return updatedMerchant ?? new ReturnApiRefatored<ClsMerchant>() { Status = "error", Messages = new List<string> { "Informações não alteradas" } };
    }

    public async Task<ReturnApiRefatored<ClsMerchant>> GetMerchantPublicAsync(string IdDoMerchant)
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsMerchant>>($"merchants/details/public/{IdDoMerchant}");
        return response ?? new ReturnApiRefatored<ClsMerchant>();
    }

    public async Task<ReturnApiRefatored<EnderecoMerchant>> CreateEnderecoParaMerchant(EnderecoMerchant novoEndereco)
    {
        var response = await _HttpClient.PostAsJsonAsync("enderecos-merchant/create", novoEndereco);

        var Resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<EnderecoMerchant>>();

        return Resultado ?? new ReturnApiRefatored<EnderecoMerchant>() { Status = "error", Messages = new List<string> {"Erro desconhecido" } };
    }
    public async Task<ReturnApiRefatored<EnderecoMerchant>> DeleteEndereco(EnderecoMerchant endereco)
    {
        var response = await _HttpClient.DeleteAsync($"enderecos-merchant/{endereco.Id}");
        var Resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<EnderecoMerchant>>();

        return Resultado ?? new ReturnApiRefatored<EnderecoMerchant>() { Status = "error", Messages = new List<string> { "Erro desconhecido" } };
    }
    public async Task<ReturnApiRefatored<EnderecoMerchant>> UpdateEndereco(EnderecoMerchant endereco)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"enderecos-merchant/enderecos/{endereco.Id}", endereco);
        var Resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<EnderecoMerchant>>();

        return Resultado ?? new ReturnApiRefatored<EnderecoMerchant>() { Status = "error", Messages = new List<string> { "Erro desconhecido" } };
    }

    public async Task<ReturnApiRefatored<DocumentosMerchant>> CreateDocumentoParaMerchant(DocumentosMerchant novoDocumento)
    {
        var response = await _HttpClient.PostAsJsonAsync("documentos-merchant/create", novoDocumento);
        var Resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<DocumentosMerchant>>();
        return Resultado ?? new ReturnApiRefatored<DocumentosMerchant>() { Status = "error", Messages = new List<string> { "Erro desconhecido" } };
    }

    public async Task<ReturnApiRefatored<DocumentosMerchant>> DeleteDocumento(DocumentosMerchant documento)
    {
        var response = await _HttpClient.DeleteAsync($"documentos-merchant/delete/{documento.Id}");

        var Resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<DocumentosMerchant>>();
        return Resultado ?? new ReturnApiRefatored<DocumentosMerchant>() { Status = "error", Messages = new List<string> { "Erro desconhecido" } };
    }

    public async Task<ReturnApiRefatored<DocumentosMerchant>> UpdateDocumento(DocumentosMerchant documento)
    {
        var response = await _HttpClient.PatchAsJsonAsync($"documentos-merchant/update/{documento.Id}", documento);
        var Resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<DocumentosMerchant>>();
        return Resultado ?? new ReturnApiRefatored<DocumentosMerchant>() { Status = "error", Messages = new List<string> { "Erro desconhecido" } };
    }
}

