namespace FrontMenuWeb.Models.Fiscal;

public class CancelaNFDto
{
    public int TipoNf { get; set; }
    public string ChNfe { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty; 
    public string Cnpj { get; set; } = string.Empty;
    public string NumeroProtocolo { get; set; } = string.Empty;
}
public class InultilizacaoNFDto
{
    public int TipoNf { get; set; }
    public int NumeroInicial { get; set; }
    public int NumeroFinal { get; set; }
    public string Justificativa { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
}

