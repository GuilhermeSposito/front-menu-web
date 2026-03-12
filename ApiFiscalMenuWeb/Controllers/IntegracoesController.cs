using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using ApiFiscalMenuWeb.Services.Integracoes;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Mvc;
using Unimake.Business.DFe.Xml.CTe;

namespace ApiFiscalMenuWeb.Controllers;

[ApiController]
[Route("integracoes")]
public class IntegracoesController : Controller
{
    private readonly IfoodServices _ifoodService;
    private readonly EmailService emailService;

    public IntegracoesController(IfoodServices ifoodService, EmailService email)
    {
        _ifoodService = ifoodService;
        emailService = email;
    }

    #region Região de Autenticacao e Autorização
    [HttpGet("ifood/authorization-code")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> GetAuthorization()
    {
        return await _ifoodService.GetAutorizationCode();
    }

    [HttpPost("ifood/autenticar")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> Autenticar([FromBody] InformacoesParaAutenticarEmpresaIfoodDto infos)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        var Return = await _ifoodService.AutenticarEmpresa(infos, token, false, null, 0);
        return Ok(Return);
    }
    #endregion

    #region Região de Polling
    [HttpGet("polling")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> Polling()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        var Return = await _ifoodService.Polling(token);
        return Ok(Return);
    }
    #endregion

    #region Região de Ações dos pedidos
    [HttpPost("despachado")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> DespachaPedido([FromBody] UpdatePedidosDto UpdateDto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        var Return = await _ifoodService.MudaStatusPedidoDespachado(UpdateDto);
        return Ok(Return);
    }

    [HttpGet("ifood/cancelation-reasons")]
    public async Task<ActionResult<ReturnApiRefatored<List<ClsCancelationReasons>>>> GetCancelationReasons([FromQuery] string IdPedidoIfood)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<List<ClsCancelationReasons>> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var Return = await _ifoodService.GetCanceletionReasons(token, IdPedidoIfood);
        return Ok(Return);
    }

    [HttpPost("ifood/cancelation")]
    public async Task<ActionResult<ReturnApiRefatored<List<ClsCancelationReasons>>>> GetCancelationReasons([FromBody] CancelationIfoodObjectDto Dto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<List<ClsCancelationReasons>> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var Return = await _ifoodService.EnviaCancelamentoDePedido(token, Dto);
        return Ok(Return);
    }

    #endregion

    [HttpPost("enviar-email-teste")]
    public async Task<ActionResult> EnviarEmailTeste()
    {

        await emailService.EnviarAsync("guilhermesposito14@gmail.com", "Este é um email de teste enviado pela API do Fiscal Menu.", "Testeeeee");
        return Ok();
    }


    [HttpPost("teste-webhook-ifood")]
    public async Task<ActionResult> TestarConexaoPorWebHook([FromBody] PollingIfoodDto PoolingIfood)
    {
        Console.WriteLine(PoolingIfood);
        Console.WriteLine("teste-webshook-chegou");
        return Ok();
    }
}
