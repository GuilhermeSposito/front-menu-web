using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FrontMenuWeb.Services;

public class PessoasService
{
    public HttpClient _HttpClient { get; set; }
    public PessoasService(HttpClient http)
    {
        _HttpClient = http;
    }

    public async Task<List<ClsPessoas>> GetPessoas()
    {
        RetornoApiPessoas? response = await _HttpClient.GetFromJsonAsync<RetornoApiPessoas>("pessoas");

        if (response is null)
            return new List<ClsPessoas>();

        if (response.Status != "success")
            return new List<ClsPessoas>();

        return response.Data.Pessoas; //se chegar aqui é pq foi 200 o retorno da api, mesmo que n tenha nenhum registro
    }

    public async Task<ClsPessoas> GetPessoaAsync(int pessoaId)
    {
        RetornoApiPessoas? response = await _HttpClient.GetFromJsonAsync<RetornoApiPessoas>($"pessoas/{pessoaId}");

        if (response is null)
            return new ClsPessoas();

        if(response.Status != "success")
            return new ClsPessoas();

        return response.Data.Pessoa; 
    }

    public async Task<RetornoApiPessoas> CadastraPessoa(ClsPessoas pessoaASerCadastrada)
    {
        HttpResponseMessage EnvioDeReq = await _HttpClient.PostAsJsonAsync("pessoas/create", pessoaASerCadastrada);

        RetornoApiPessoas? response = await EnvioDeReq.Content.ReadFromJsonAsync<RetornoApiPessoas>();

        return response!;

       /* if (response is null)
            return "Erro ao cadastrar pessoa;";

        if (response.Status != "success")
             return response.message ?? "Erro ao cadastrar pessoa;";

        return response.Data.Message ?? "Pessoa cadastrada com sucesso!";*/

    }

    public async Task<string> UpdatePessoa(ClsPessoas pessoaASerCadastrada)
    {
        HttpResponseMessage EnvioDeReq = await _HttpClient.PatchAsJsonAsync($"pessoas/update/{pessoaASerCadastrada.Id}", pessoaASerCadastrada);

        RetornoApiPessoas? response = await EnvioDeReq.Content.ReadFromJsonAsync<RetornoApiPessoas>();

        if (response is null)
            return "Erro ao atualizar pessoa;";

        if (response.Status != "success")
             return response.message ?? "Erro ao atualizar pessoa;";

        return response.Data.Message ?? "Pessoa atualizar com sucesso!";

    }

    public async Task<RetornoApiPessoas> DeletePessoa(ClsPessoas pessoaASerCadastrada)
    {
        HttpResponseMessage EnvioDeReq = await _HttpClient.DeleteAsync($"pessoas/delete/{pessoaASerCadastrada.Id}");

        RetornoApiPessoas? response = await EnvioDeReq.Content.ReadFromJsonAsync<RetornoApiPessoas>();

        return response!;
  
    }


    //a partir daqui é os endpoits de endereços de pessoas

    public async Task<EnderecoPessoa> GetEndereco(int idendereco)
    {
        RetornoApiPessoas? response = await _HttpClient.GetFromJsonAsync<RetornoApiPessoas>($"pessoas/endereco/{idendereco}");

        if (response is null)
            return new EnderecoPessoa();

        if (response.Status != "success")
            return new EnderecoPessoa();

        return response.Data.Enderecos;
    }

    public async Task<string> CadastraEndereco(EnderecoPessoa EnderecoASerCadastrado, int IdPessoa)
    {
        HttpResponseMessage EnvioDeReq = await _HttpClient.PostAsJsonAsync($"pessoas/endereco/create/{IdPessoa}", EnderecoASerCadastrado);

        RetornoApiPessoas? response = await EnvioDeReq.Content.ReadFromJsonAsync<RetornoApiPessoas>();

        if (response is null)
            return "Erro ao cadastrar Endereço;";

        if (response.Status != "success")
            return response.message ?? "Erro ao cadastrar Endereço;";

        return response.Data.Message ?? "Endereço cadastrada com sucesso!";

    }

    public async Task<string> UpdateEndereco(EnderecoPessoa EnderecoASerModificado,int idDaPessoaQueTemOEnderecoCadastrado)
    {
        HttpResponseMessage EnvioDeReq = await _HttpClient.PatchAsJsonAsync($"pessoas/{idDaPessoaQueTemOEnderecoCadastrado}/endereco/update/{EnderecoASerModificado.Id}", EnderecoASerModificado);

        RetornoApiPessoas? response = await EnvioDeReq.Content.ReadFromJsonAsync<RetornoApiPessoas>();

        if (response is null)
            return "Erro ao atualizar Endereço;";

        if (response.Status != "success")
            return response.message ?? "Erro ao atualizar Endereço;";

        return response.Data.Message ?? "Endereço atualizado com sucesso!";

    }


    public async Task<string> DeleteEndereco(EnderecoPessoa EnderecoASerModificado, int idDaPessoaQueTemOEnderecoCadastrado)
    {
        HttpResponseMessage EnvioDeReq = await _HttpClient.DeleteAsync($"pessoas/{idDaPessoaQueTemOEnderecoCadastrado}/endereco/delete/{EnderecoASerModificado.Id}");


        RetornoApiPessoas? response = await EnvioDeReq.Content.ReadFromJsonAsync<RetornoApiPessoas>();

        if (response is null)
            return "Erro ao deletar endereço;";

        if (response.Status != "success")
            return response.message ?? "Erro ao deletar endereço;";

        return response.Data.Message ?? "Endereço deletado com sucesso!";

    }

}
