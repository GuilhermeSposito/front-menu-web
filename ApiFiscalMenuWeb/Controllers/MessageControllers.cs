using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
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

    public MessageControllers(MessageService nfService)
    {
        _messageService = nfService;
    }


}