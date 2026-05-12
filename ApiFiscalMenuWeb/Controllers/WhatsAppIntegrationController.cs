using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using ApiFiscalMenuWeb.Services.Integracoes;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ApiFiscalMenuWeb.Controllers;

[ApiController]
[Route("whats_app")]
public class WhatsAppIntegrationController : Controller
{
    private readonly WebhookSignature _webhookSignature;
    private readonly IConfiguration _configuration;
    private readonly MessageService _messageService;
    private readonly WhatsAppOptInService _optInService;

    public WhatsAppIntegrationController(
        WebhookSignature webhookSignature,
        IConfiguration configuration,
        MessageService messageService,
        WhatsAppOptInService optInService)
    {
        _webhookSignature = webhookSignature;
        _configuration = configuration;
        _messageService = messageService;
        _optInService = optInService;
    }

    #region Opt-in / Opt-out

    // Público (sem Bearer) — usado pelo cardápio online para registrar consentimento do cliente.
    [HttpPost("optin")]
    public async Task<IActionResult> RegistrarOptIn([FromBody] WhatsAppOptInRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MerchantId) || string.IsNullOrWhiteSpace(dto.Telefone))
            return BadRequest("merchantId e telefone são obrigatórios.");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _optInService.RegistrarOptInAsync(dto.MerchantId, dto.Telefone, dto.OptedInVia ?? "CARDAPIO", ip);
        return Ok(new { status = "success" });
    }

    // Requer x-api-key — chamado pela NestAPI ou internamente para registrar opt-out.
    [HttpPut("optout")]
    public async Task<IActionResult> RegistrarOptOut([FromBody] WhatsAppOptOutRequestDto dto)
    {
        if (!ValidarApiKey()) return Unauthorized("x-api-key inválida ou ausente.");
        if (string.IsNullOrWhiteSpace(dto.MerchantId) || string.IsNullOrWhiteSpace(dto.Telefone))
            return BadRequest("merchantId e telefone são obrigatórios.");

        await _optInService.RegistrarOptOutAsync(dto.MerchantId, dto.Telefone);
        return Ok(new { status = "success" });
    }

    private bool ValidarApiKey()
    {
        var apiKey = Request.Headers["x-api-key"].ToString();
        var expected = _configuration["ApiKeyNest"] ?? "";
        return !string.IsNullOrEmpty(apiKey) && apiKey == expected;
    }

    #endregion

    #region Região de Webhooks (Pontos de extremidade)

    [HttpGet("endpoint-webhook")]
    public IActionResult EndpointDeConexaoComWebHook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.challenge")] string challenge,
        [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        if (mode == "subscribe" && verifyToken == "token")
            return Content(challenge, "text/plain");

        return Unauthorized();
    }

    [HttpPost("endpoint-webhook")]
    public async Task<IActionResult> EndpointDeConexaoComWebHookPost()
    {
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        var bodyBytes = ms.ToArray();

        var signatureHeader = Request.Headers["X-Hub-Signature-256"].ToString();
        if (string.IsNullOrEmpty(signatureHeader) || !signatureHeader.StartsWith("sha256="))
            return Unauthorized();

        var signature = signatureHeader["sha256=".Length..];
        var appSecret = _configuration["MetaWebhookSecret"] ?? "";

        if (!_webhookSignature.ValidateSignature(appSecret, bodyBytes, signature))
            return Unauthorized();

        var rawBody = Encoding.UTF8.GetString(bodyBytes);
        var dto = JsonSerializer.Deserialize<WhatsAppWebhookDto>(rawBody);
        if (dto != null)
            _ = _messageService.ProcessarMensagemRecebidaAsync(dto);

        return Ok();
    }

    [HttpPost("data-deletion")]
    public IActionResult DataDeletion([FromForm(Name = "signed_request")] string signedRequest)
    {
        if (string.IsNullOrEmpty(signedRequest))
            return BadRequest();

        var appSecret = _configuration["MetaWebhookSecret"] ?? "";
        var result = _messageService.ProcessarSolicitacaoDeDelecaoDados(signedRequest, appSecret);

        return Ok(result);
    }

    #endregion
}
