using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Authorization;
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

    #region Verificação de status da NFe e NFCe

    [HttpPost("status-nfe")]
    public async Task<ActionResult> VerificarStatusNFe()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var result = await _nfService.VerificaStatusDaNFe(token);
        return Ok(result);
    }

    [HttpPost("status-nfce")]
    public async Task<ActionResult> VerificarStatusNFCe()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var result = await _nfService.VerificaStatusDaNFCe(token);
        return Ok(result);
    }
    #endregion

    #region Envio de NFCe e NFe
    [HttpPost("enviar-nfe")]
    public async Task<ActionResult> EnviarNFe([FromBody] ClsPedido Pedido)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var result = await _nfService.EmissaoDeNFe(token, Pedido);
        return Ok(result);
    }

    [HttpPost("enviar-nfce")]
    public async Task<ActionResult> EnviarNFCe([FromBody] EnNfCeDto EnvNFCeDto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var result = await _nfService.EmissaoDeNFCe(token, EnvNFCeDto);
        return Ok(result);
    }
    #endregion
}