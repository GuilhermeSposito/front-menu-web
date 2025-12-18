using Microsoft.AspNetCore.Components;

namespace FrontMenuWeb.DTOS;

public class SuporteDeImpressaoDeQrCode
{
    public int IdMesa { get; set; }
    public string CodigoExternoMesa { get; set; } = string.Empty;
    public MarkupString QrCodeSvg { get; set; }
}
