using System.ComponentModel.DataAnnotations.Schema;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class InformacoesDoTokenRetornadaPeloIfoodDto
{
    [Column("accesstoken")] public string? AccessToken { get; set; }
    [Column("refreshtoken")] public string? RefreshToken { get; set; }
    [Column("expiresin")] public int ExpiresIn { get; set; }

}
