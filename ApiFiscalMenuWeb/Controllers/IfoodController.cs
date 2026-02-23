using ApiFiscalMenuWeb.Services.Integracoes;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Mvc;

namespace ApiFiscalMenuWeb.Controllers;

[ApiController]
[Route("ifood")]
public class IfoodController : Controller
{
    private readonly IfoodServices _ifoodService;

    public IfoodController(IfoodServices ifoodService)
    {
        _ifoodService = ifoodService;
    }

    #region Região de Autenticacao e Autorização
    [HttpGet("authorization-code")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> GetAuthorization()
    {
        return await _ifoodService.GetAutorizationCode();
    }

    [HttpPost("autenticar")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> Autenticar([FromBody] InformacoesParaAutenticarEmpresaIfoodDto infos)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        var Return =  await _ifoodService.AutenticarEmpresa(infos, token);
        return Ok(Return);
    }
    #endregion
}
