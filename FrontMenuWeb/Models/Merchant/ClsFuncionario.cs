using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsFuncionario
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("cpf")] public string CPF { get; set; } = string.Empty;
    [JsonPropertyName("funcao")] public string Funcao { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("senha")] public string Senha { get; set; } = string.Empty;
    [JsonPropertyName("salario")] public float Salario { get; set; } = 0f;
    [JsonPropertyName("ultimoPag")] public DateTime? UltimoPagamento { get; set; } 
    [JsonPropertyName("modificadoEm")] public DateTime? ModificadoEm { get; set; } 
    [JsonPropertyName("adimissao")] public DateTime? Admissao { get; set; } 
    [JsonPropertyName("demissao")] public DateTime? Demissao { get; set; } 
    [JsonPropertyName("motivoDemissao")] public string motivoDemissao { get; set; } = string.Empty;
    [JsonPropertyName("DiaPagamento")] public int DiaDoPagamento { get; set; }
    [JsonPropertyName("celular")] public string Celular { get; set; } = string.Empty;
    [JsonPropertyName("rua")] public string Rua { get; set; } = string.Empty;
    [JsonPropertyName("bairro")] public string Bairro { get; set; } = string.Empty;
    [JsonPropertyName("cidade")] public string Cidade { get; set; } = string.Empty;
    [JsonPropertyName("cep")] public string Cep { get; set; } = string.Empty;
    [JsonPropertyName("usaOSistema")] public bool usaOSistema { get; set; } = false;
    [JsonPropertyName("AcessoDashboard")] public bool AcessoDashboard { get; set; } = true;
    [JsonPropertyName("AcessoCadastroProduto")] public bool AcessoCadastroProduto { get; set; } = true;
    [JsonPropertyName("AcessoCadastroGrupoProduto")] public bool AcessoCadastroGrupoProduto { get; set; } = true;
    [JsonPropertyName("AcessoEditarProduto")] public bool AcessoEditarProduto { get; set; } = true;
    [JsonPropertyName("AcessoDeletaProduto")] public bool AcessoDeletaProduto { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDeAliquotas")] public bool AcessoCadastroDeAliquotas { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDePessoa")] public bool AcessoCadastroDePessoa { get; set; } = true;
    [JsonPropertyName("AcessoCadastraMesaOuComandas")] public bool AcessoCadastraMesaOuComandas { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDePromocoes")] public bool AcessoCadastroDePromocoes { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDeGarcons")] public bool AcessoCadastroDeGarcons { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDeMotoboy")] public bool AcessoCadastroDeMotoboy { get; set; } = true;
    [JsonPropertyName("AcessoFinanceiro")] public bool AcessoFinanceiro { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDeContas")] public bool AcessoCadastroDeContas { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDeCategoriasFinanceiro")] public bool AcessoCadastroDeCategoriasFinanceiro { get; set; } = true;
    [JsonPropertyName("AcessoCadastroFormasDeRecebimento")] public bool AcessoCadastroFormasDeRecebimento { get; set; } = true;
    [JsonPropertyName("AcessoCadastraFormaDePagamento")] public bool AcessoCadastraFormaDePagamento { get; set; } = true;
    [JsonPropertyName("AcessoLancamentosFinanceiro")] public bool AcessoLancamentosFinanceiro { get; set; } = true;
    [JsonPropertyName("AcessoCadastraLancamento")] public bool AcessoCadastraLancamento { get; set; } = true;
    [JsonPropertyName("AcessoEstatisticas")] public bool AcessoEstatisticas { get; set; } = true;
    [JsonPropertyName("AcessoVendas")] public bool AcessoVendas { get; set; } = true;
    [JsonPropertyName("AcessoLancaVenda")] public bool AcessoLancaVenda { get; set; } = true;
    [JsonPropertyName("AcessoCancelaVenda")] public bool AcessoCancelaVenda { get; set; } = true;
    [JsonPropertyName("AcessoEditaVenda")] public bool AcessoEditaVenda { get; set; } = true;
    [JsonPropertyName("AcessoCancelaItensDaVenda")] public bool AcessoCancelaItensDaVenda { get; set; } = true;
    [JsonPropertyName("AcessoConfiguracoes")] public bool AcessoConfiguracoes { get; set; } = true;
    [JsonPropertyName("AcessoConfigsDeImpressao")] public bool AcessoConfigsDeImpressao { get; set; } = true;
    [JsonPropertyName("AcessoCadastroDeFuncionarios")] public bool AcessoCadastroDeFuncionarios { get; set; } = true;
    [JsonPropertyName("ativo")] public bool ativo { get; set; } = true;

}
