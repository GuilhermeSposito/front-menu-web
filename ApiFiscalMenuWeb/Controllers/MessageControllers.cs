using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Fiscal;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFiscalMenuWeb.Controllers;

[ApiController]
[Route("message")]
public class MessageControllers : ControllerBase
{
    private readonly MessageService _messageService;

    public MessageControllers(MessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost("enviar-msg")]
    public async Task<ActionResult> EnviaMsgWhatsApp([FromBody] EnviaMsgDto enviaMsgDto)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = HttpContext.Request.Cookies["auth_token"] ?? authHeader.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ReturnApiRefatored<ClsPedido> { Status = "error", Messages = new List<string> { "Cookie auth_token não encontrado" } });


        await _messageService.SendMessageAsync(enviaMsgDto,TokenDaApiNest: token);
        return Ok();
    }
}