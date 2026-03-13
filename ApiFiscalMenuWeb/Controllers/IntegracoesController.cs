using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using ApiFiscalMenuWeb.Services.Integracoes;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Unimake.Business.DFe.Xml.CTe;

namespace ApiFiscalMenuWeb.Controllers;

[ApiController]
[Route("integracoes")]
public class IntegracoesController : Controller
{
    private readonly IfoodServices _ifoodService;
    private readonly EmailService emailService;
    private WebhookSignature _webhookSignature;

    public IntegracoesController(IfoodServices ifoodService, EmailService email, WebhookSignature webhookSignature)
    {
        _ifoodService = ifoodService;
        emailService = email;
        _webhookSignature = webhookSignature;
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

        await emailService.EnviarAsync("guilherme@sophos-erp.com.br", "Este é um email de teste enviado pela API do Fiscal Menu.", "Testeeeee");
        return Ok();
    }


    [HttpPost("endpoint-webhook-ifood")]
    public async Task<IActionResult> IfoodConexaoPorWebHook()
    {
        HttpContext.Request.EnableBuffering();

        var signature = HttpContext.Request.Headers["X-IFood-Signature"].ToString();

        Console.WriteLine(signature);

        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        HttpContext.Request.Body.Position = 0;

        var secret = "4kyv4yt3b2cczztrdfihr8pihblgptoa9a5pw9ldmeq7tidz90nauhp2009opffjoh33ay1uy60unq3gw1vm8u72dm91ols7fry";

        Console.WriteLine(body);

        bool valid = _webhookSignature.ValidateSignature(secret, body, signature);

        if (!valid)
        {
            Console.WriteLine("IFOOD NÃO AUTORIZADO");
            return Unauthorized();
        }

        var dto = JsonSerializer.Deserialize<PollingIfoodDto>(body);

        return Accepted();
    }
}
