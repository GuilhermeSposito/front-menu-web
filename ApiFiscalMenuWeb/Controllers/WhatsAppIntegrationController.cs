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
[Route("whats_app")]
public class WhatsAppIntegrationController : Controller
{
    private readonly EmailService emailService;
    private WebhookSignature _webhookSignature;
    private readonly IConfiguration _configuration;

    public WhatsAppIntegrationController(EmailService email, WebhookSignature webhookSignature, IConfiguration configuration)
    {
        emailService = email;
        _webhookSignature = webhookSignature;
        _configuration = configuration;
    }

    #region Região de Webhooks (Pontos de extremidade)
    [HttpGet("endpoint-webhook")]
    public IActionResult EndpointDeConexaoComWebHook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.challenge")] string challenge,
        [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        if (mode == "subscribe" && verifyToken == "token")
        {
            return Content(challenge, "text/plain");
        }

        return Unauthorized();
    }

    [HttpPost("endpoint-webhook")]
    public async Task<IActionResult> EndpointDeConexaoComWebHookPost()
    {
        var signature = HttpContext.Request.Headers["HUB.VERIFY_TOKEN"].ToString();
        if (signature == "token")
            return Ok();

        return Ok();
    }

    #endregion


}
