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

    #endregion


    [HttpPost("endpoint-webhook-ifood")]
    public async Task<IActionResult> EndpointDeConexaoIfoodWebHook()
    {
        var secret = "4kyv4yt3b2cczztrdfihr8pihblgptoa9a5pw9ldmeq7tidz90nauhp2009opffjoh33ay1uy60unq3gw1vm8u72dm91ols7fry";
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
        var json = JsonSerializer.Serialize(dto);
        Console.WriteLine($"Body: {json}");
        Console.WriteLine($"Ifood Signature {signature}");

        if (dto is not null)
            await _ifoodService.AddOrUpdateOrders(dto);

        return Accepted(new { dto?.MerchantIds });
    }
}
