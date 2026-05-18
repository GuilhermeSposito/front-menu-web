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
    private readonly B1DeliveryServices _delmatchService;
    private readonly NestApiServices _nestApiService;
    private readonly EmailService emailService;
    private WebhookSignature _webhookSignature;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IntegracoesController> _logger;

    public IntegracoesController(
        IfoodServices ifoodService,
        B1DeliveryServices delmatchService,
        NestApiServices nestApiService,
        EmailService email,
        WebhookSignature webhookSignature,
        IConfiguration configuration,
        ILogger<IntegracoesController> logger)
    {
        _ifoodService = ifoodService;
        _delmatchService = delmatchService;
        _nestApiService = nestApiService;
        emailService = email;
        _webhookSignature = webhookSignature;
        _configuration = configuration;
        _logger = logger;
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

    [HttpPost("delmatch/accepted/{idIntegracao}")]
    public async Task<ActionResult> AceitaPedidoDelmatch([FromRoute] string idIntegracao, [FromQuery] string merchantSophosId)
    {
        var sucesso = await _delmatchService.AceitarPedidoManual(idIntegracao, merchantSophosId);
        return Ok(new { Status = sucesso ? "success" : "error", Messages = new List<string> { sucesso ? "Pedido aceito com sucesso" : "Erro ao aceitar pedido Delmatch" } });
    }
    #endregion

    #region Região de Webhooks (Pontos de extremidade)
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
        if (!valid)
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

    [HttpPost("endpoint-webhook-b1-delivery")]
    public IActionResult EndpointDeConexaoB1Delivery([FromBody] DelmatchWebhookDto webhook)
    {
        if (webhook is null)
            return BadRequest();

        _ = _delmatchService.ProcessarWebhookAsync(webhook);
        return Accepted();
    }

    [HttpPost("endpoint-webhook-anotaai/{IdMerchant}")]
    public async Task<IActionResult> EndpointDeConexaoAnotaAi([FromRoute] string IdMerchant, [FromBody] AnotaAiOrderInfoDto PedidoAnotaAi)
    {
        Console.WriteLine(PedidoAnotaAi.Id + PedidoAnotaAi.Items[0].Name);

        return Accepted();
    }
    #endregion

    [HttpGet("teste-polling")]
    public async Task<ActionResult> TestePolling()
    {
        await _ifoodService.PollingIfood();
        return Ok();
    }

}
