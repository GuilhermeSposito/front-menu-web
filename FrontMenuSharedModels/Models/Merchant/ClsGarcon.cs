using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Merchant;

public class ClsGarcon
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("cpf")] public string CPF { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("senha")] public string Senha { get; set; } = string.Empty;
    [JsonPropertyName("nome_usuario")] public string NomeUsuario { get; set; } = string.Empty;
    [JsonPropertyName("valor_vendas")] public decimal ValorVendas { get; set; } = 0;
    [JsonPropertyName("valor_taxas_servico")] public decimal ValorTaxasServico { get; set; } = 0;
    [JsonPropertyName("merchantId")] public string MerchantId { get; set; } = string.Empty;
}
