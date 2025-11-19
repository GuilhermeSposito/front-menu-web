using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiFiscalMenuWeb.Controllers;

[ApiController]
[Route("nf")]
public class NfControllers : ControllerBase
{
    private readonly NfService _nfService;

    public NfControllers(NfService nfService)
    {
        _nfService = nfService;
    }


    [HttpPost("status")]
    public async Task<ActionResult> VerificarStatus()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var result = await _nfService.VerificaStatusDoCertificadoDigital(token);
        return Ok(result);
    }
}

//https://syslogicadev.com/apifiscal/nf/status