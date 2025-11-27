namespace FrontMenuWeb.Models.Fiscal;

public class NfeReturnDto
{
    public int? NFTipo { get; set; } = 65; //65 para NFCe e 55 para NFe
    public string? ChaveNf { get; set; }
    public int Cstat { get; set; }
    public string? Xmotivo { get; set; }
    public string? XmlStringField { get; set; }
}
