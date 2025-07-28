using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Pessoas;

public class ClsPessoas
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("tipo_pessoa_id")] public int TipoPessoaId { get; set; }
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("nome_fantasia")] public string? NomeFantasia { get; set; }
    [JsonPropertyName("razao_social")] public string? RazaoSocial { get; set; }
    [JsonPropertyName("cpf")] public string? Cpf { get; set; }
    [JsonPropertyName("cnpj")] public string? Cnpj { get; set; }
    [JsonPropertyName("telefone")] public string? Telefone { get; set; }
    [JsonPropertyName("telefone1")] public string? Telefone1 { get; set; }
    [JsonPropertyName("telefone2")] public string? Telefone2 { get; set; }
    [JsonPropertyName("contato")] public string? Contato { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("senha")] public string? Senha { get; set; }
    [JsonPropertyName("site")] public string? Site { get; set; }
    [JsonPropertyName("nascimento")] public string? Nascimento { get; set; }
    [JsonPropertyName("refere")] public string? Refere { get; set; }
    [JsonPropertyName("venda")] public string? Venda { get; set; }
    [JsonPropertyName("obs")] public string? Observacao { get; set; }
    [JsonPropertyName("setor")] public string? Setor { get; set; }
    [JsonPropertyName("desconto")] public int? Desconto { get; set; }


}


public class RetornoApiPessoas
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string? message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public Data Data { get; set; } = new Data();
}

public class  Data
{
    [JsonPropertyName("pessoas")]public List<ClsPessoas> Pessoas { get; set; } = new List<ClsPessoas>();
    [JsonPropertyName("pessoa")]public ClsPessoas Pessoa { get; set; } = new ClsPessoas();
    [JsonPropertyName("message")] public string? Message { get; set; } = string.Empty;
    [JsonPropertyName("endereco")] public EnderecoPessoa Enderecos { get; set; } = new EnderecoPessoa();


}

public class EnderecoPessoa
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("rua")] public string Rua { get; set; } = string.Empty;
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("bairro")] public string Bairro { get; set; } = string.Empty;
    [JsonPropertyName("cidade")] public string Cidade { get; set; } = string.Empty;
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("cep")] public string? Cep { get; set; }
    [JsonPropertyName("referencia")] public string? Referencia { get; set; }
    [JsonPropertyName("complemento")] public string? Complemento { get; set; }
    [JsonPropertyName("obs_endereco")] public string? ObsEndereco { get; set; }
    [JsonPropertyName("tipo_endereco")] public string? TipoEndereco { get; set; }

}