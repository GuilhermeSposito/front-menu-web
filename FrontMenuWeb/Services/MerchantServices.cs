using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services;

public class MerchantServices
{
    public HttpClient _HttpClient { get; set; }
    private readonly AppState _appState;

    public MerchantServices(HttpClient http, AppState appState)
    {
        _HttpClient = http;
        _appState = appState;
    }

    public async Task<bool> CheckPermissionAsync(string action)
    {
        // Se não for funcionário (é o proprietário), tem permissão total
        if (!_appState.IsFuncionario) return true;

        try
        {
            var response = await _HttpClient.GetFromJsonAsync<FrontMenuWeb.DTOS.CheckPermissionResponse>($"merchants/check-permission/{action}");
            return response?.Allowed ?? false;
        }
        catch
        {
            return false;
        }
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

    public async Task<ReturnApiRefatored<ClsMerchant>> AdminGetAllMerchantsAsync()
    {
        var response = await _HttpClient.GetFromJsonAsync<ReturnApiRefatored<ClsMerchant>>("merchants/all");
        return response ?? new ReturnApiRefatored<ClsMerchant>();
    }

    public async Task<ReturnApiRefatored<ClsMerchant>> AdminUpdateMerchantStatusAsync(string merchantId, bool ativo, DateTime ativoAte)
    {
        var payload = new Dictionary<string, object>
        { 
            { "ativo", ativo }, 
            { "AtivoAte", ativoAte.ToString("yyyy-MM-ddTHH:mm:ssZ") } 
        };
        var response = await _HttpClient.PatchAsJsonAsync($"merchants/admin/update-status/{merchantId}", payload);
        var result = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMerchant>>();
        return result ?? new ReturnApiRefatored<ClsMerchant>() { Status = "error", Messages = new List<string> { "Erro ao atualizar status" } };
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

    public async Task<ReturnApiRefatored<ClsMerchant>> EnviarArquicosParaContabilidadeEmail(MemoryStream ms,string filename , string? emailSelecionado = "guilhermesposito14@gmail.com")
    {
        using var content = new MultipartFormDataContent();

        // Arquivo
        var fileContent = new ByteArrayContent(ms.ToArray());
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        content.Add(fileContent, "arquivos", filename);

        var response = await _HttpClient.PostAsync(
            $"envios-email/arquivos-fiscais?email={emailSelecionado}",
            content
        );

        var resultado =
            await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsMerchant>>();

        return resultado ??
            new ReturnApiRefatored<ClsMerchant>
            {
                Status = "error",
                Messages = new List<string> { "Erro desconhecido" }
            };
    }

    public async Task<ReturnApiRefatored<ClsProduto>> AdicionarFoto(MemoryStream ms, string filename, string IdMerchant)
    {
        using var content = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(ms.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "foto", filename);
        var response = await _HttpClient.PatchAsync($"merchants/update/imagem/{IdMerchant}", content);

        var resultado = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<ClsProduto>>();

        return resultado ??
            new ReturnApiRefatored<ClsProduto>
            {
                Status = "error",
                Messages = new List<string> { "Erro desconhecido" }
            };
    }

    public async Task<ClsMerchant?> GetSessionInfoAsync()
    {
        return await _HttpClient.GetFromJsonAsync<ClsMerchant>("merchants/session-info");
    }
}

