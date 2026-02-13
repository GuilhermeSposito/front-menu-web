using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Fiscal;
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

    [HttpGet("status-nfe")]
    public async Task<ActionResult> VerificarStatusNFe()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();//var token = authHeader.Replace("Bearer ", "");
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var result = await _nfService.VerificaStatusDaNFe(token);
        return Ok(result);
    }

    [HttpGet("status-nfce")]
    public async Task<ActionResult> VerificarStatusNFCe()
    {

        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();//var token = authHeader.Replace("Bearer ", "");
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", ""); 
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var result = await _nfService.VerificaStatusDaNFCe(token);
        return Ok(result);
    }
    #endregion

    #region Envio de NFCe e NFe
    [HttpPost("enviar-nfe")]
    public async Task<ActionResult> EnviarNFe([FromBody] ClsPedido Pedido)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();//var token = authHeader.Replace("Bearer ", "");
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var result = await _nfService.EmissaoDeNFe(token, Pedido);
        return Ok(result);
    }

    [HttpPost("enviar-nfce")]
    public async Task<ActionResult> EnviarNFCe([FromBody] EnNfCeDto EnvNFCeDto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();//var token = authHeader.Replace("Bearer ", "");
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });

        var result = await _nfService.EmissaoDeNFCe(token, EnvNFCeDto);
        return Ok(result);
    }
    #endregion

    #region Cancelamento de NFe e NFCe
    [HttpPost("cancelar-nfe")]
    public async Task<ActionResult> CancelarNFce([FromBody] CancelaNFDto CancelNfDto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();//var token = authHeader.Replace("Bearer ", "");
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        var result = await _nfService.CancelamentoDeNFCe(token, CancelNfDto);
        return Ok(result);
    }

    [HttpPost("inutilizar-nfce")]
    public async Task<ActionResult> InultilizacaoDeNFCe([FromBody] InultilizacaoNFDto InuDto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();//var token = authHeader.Replace("Bearer ", "");
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        var result = await _nfService.InultilizacaoDeNFCe(token, InuDto);
        return Ok(result);
    }

    #endregion
}