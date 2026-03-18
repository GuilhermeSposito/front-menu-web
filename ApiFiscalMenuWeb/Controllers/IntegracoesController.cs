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
    private readonly IConfiguration _configuration;

    public IntegracoesController(IfoodServices ifoodService, EmailService email, WebhookSignature webhookSignature, IConfiguration configuration)
    {
        _ifoodService = ifoodService;
        emailService = email;
        _webhookSignature = webhookSignature;
        _configuration = configuration;
    }


    #region Região de Polling
    [HttpGet("polling")]
    public async Task<ActionResult<ReturnApiRefatored<object>>> Polling()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        // var Return = await _ifoodService.Polling(token);
        return Ok();
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

    [HttpPost("ifood/accepted/{id}")]
    public async Task<ActionResult> AceitaPedido([FromRoute] string id)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<object> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var Return = await _ifoodService.AceitaPedido(id);
        return Ok(Return);
    }
    #endregion


    [HttpPost("endpoint-webhook-ifood")]
    public async Task<IActionResult> EndpointDeConexaoIfoodWebHook()
    {
        var secret = _configuration["Ifood:ClientSecret"];
        HttpContext.Request.EnableBuffering();

        using var ms = new MemoryStream();
        await HttpContext.Request.Body.CopyToAsync(ms);

        var bodyBytes = ms.ToArray();
        var body = Encoding.UTF8.GetString(bodyBytes);

        var signature = HttpContext.Request.Headers["X-IFood-Signature"].ToString();
        var dto = JsonSerializer.Deserialize<WebHookIfoodDto>(bodyBytes);


        var valid = _webhookSignature.ValidateSignature(secret, bodyBytes, signature);
        if (!valid && !dto!.Teste)
        {
            if (dto?.FullCode == "KEEPALIVE")
                return Accepted(new { dto?.MerchantIds });

            Console.WriteLine("Assinatura Invalida");
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Assinatura inválida" } });
        }


        if (dto is not null && dto.FullCode != "KEEPALIVE")
            await _ifoodService.AddOrUpdateOrders(dto);

        return Accepted(new { dto?.MerchantIds });
    }

    [HttpPost("autenticar_ifood_teste")]
    public async Task<ActionResult> AutenticarEmpresa()
    {
        await _ifoodService.AutenticarEmpresa();

        return Ok();
    }
}
