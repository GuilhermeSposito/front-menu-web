using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class AvisoContaItemDto
{
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = "";
    [JsonPropertyName("Quantidade")] public int Quantidade { get; set; }
    [JsonPropertyName("PrecoUnitario")] public float PrecoUnitario { get; set; }
    [JsonPropertyName("PrecoTotal")] public float PrecoTotal { get; set; }
    [JsonPropertyName("LegTamanhoEscolhido")] public string? LegTamanhoEscolhido { get; set; }
    [JsonPropertyName("NomeCliente")] public string? NomeCliente { get; set; }
    [JsonPropertyName("Complementos")] public List<AvisoContaComplementoDto>? Complementos { get; set; }
}

public class AvisoContaComplementoDto
{
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = "";
    [JsonPropertyName("Quantidade")] public int Quantidade { get; set; }
    [JsonPropertyName("PrecoUnitario")] public float PrecoUnitario { get; set; }
}

public class AvisoContaTotaisDto
{
    [JsonPropertyName("Subtotal")] public float Subtotal { get; set; }
    [JsonPropertyName("TaxaDeServico")] public float TaxaDeServico { get; set; }
    [JsonPropertyName("CouvertIndividual")] public float CouvertIndividual { get; set; }
    [JsonPropertyName("Total")] public float Total { get; set; }
}

public class AvisoContaCouvertClienteDto
{
    [JsonPropertyName("Nome")] public string Nome { get; set; } = "";
    [JsonPropertyName("Qtd")] public int Qtd { get; set; }
    [JsonPropertyName("Valor")] public float Valor { get; set; }
}

public class AvisoContaCouvertDto
{
    [JsonPropertyName("ValorPorPessoa")] public float ValorPorPessoa { get; set; }
    [JsonPropertyName("QtdPessoas")] public int QtdPessoas { get; set; }
    [JsonPropertyName("PorCliente")] public List<AvisoContaCouvertClienteDto>? PorCliente { get; set; }
    [JsonPropertyName("Avulso")] public int? Avulso { get; set; }
}

public class AvisoContaComandaDto
{
    [JsonPropertyName("Nome")] public string? Nome { get; set; }
    [JsonPropertyName("Itens")] public List<AvisoContaItemDto> Itens { get; set; } = new();
    [JsonPropertyName("Totais")] public AvisoContaTotaisDto Totais { get; set; } = new();
}

public class AvisarContaRequestDto
{
    [JsonPropertyName("MesaId")] public int MesaId { get; set; }
    [JsonPropertyName("Itens")] public List<AvisoContaItemDto> Itens { get; set; } = new();
    [JsonPropertyName("CobraTaxaServico")] public bool CobraTaxaServico { get; set; }
    [JsonPropertyName("CobraCouvert")] public bool CobraCouvert { get; set; }
    [JsonPropertyName("SepararPorCliente")] public bool SepararPorCliente { get; set; }
    [JsonPropertyName("QtdPessoas")] public int? QtdPessoas { get; set; }
}

public class AvisoContaDto
{
    [JsonPropertyName("Mesa")] public int Mesa { get; set; }
    [JsonPropertyName("MesaId")] public int MesaId { get; set; }
    [JsonPropertyName("SepararPorCliente")] public bool SepararPorCliente { get; set; }
    [JsonPropertyName("Comandas")] public List<AvisoContaComandaDto>? Comandas { get; set; }
    [JsonPropertyName("Itens")] public List<AvisoContaItemDto>? Itens { get; set; }
    [JsonPropertyName("SubtotalDaMesa")] public float SubtotalDaMesa { get; set; }
    [JsonPropertyName("TaxaDeServicoDaMesa")] public float TaxaDeServicoDaMesa { get; set; }
    [JsonPropertyName("CouvertTotalMesa")] public float CouvertTotalMesa { get; set; }
    [JsonPropertyName("CouvertAvulso")] public float CouvertAvulso { get; set; }
    [JsonPropertyName("Couvert")] public AvisoContaCouvertDto? Couvert { get; set; }
    [JsonPropertyName("TotalGeral")] public float TotalGeral { get; set; }
}
