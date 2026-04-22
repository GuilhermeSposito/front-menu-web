using FrontMenuWeb.Models.Roteirizacao;

namespace FrontMenuWeb.DTOS;

public class EnviaMsgMotoboyDto
{
    public string NomeMotoboy { get; set; } = string.Empty;
    public string TelefoneMotoboy { get; set; } = string.Empty;
    public string LinkGoogleMaps { get; set; } = string.Empty;
    public List<PedidoParaRota> Paradas { get; set; } = new();
}
