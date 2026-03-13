using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    private readonly EmailService emailService;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, EmailService email)
    {
        _logger = logger;
        emailService = email;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception.ToString());

        // Dispara envio em background
        _ = Task.Run(async () =>
        {
            try
            {
                var ex = context.Exception;

                var logFormatado = $"""
                    [{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}] {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}

                    Status: 500
                    Mensagem: {ex.Message}

                    Stack: {ex.StackTrace}
                    """;

                await emailService.EnviarAsync(
                    "guilherme@sophos-erp.com.br", 
                    $"Log De Erro Na APi {DateTime.Now:g}",
                    logFormatado);


            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao enviar email de exceção: " + ex.Message);
            }
        });


        int statusCode = context.Exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Result = new ObjectResult(
            new ReturnApiRefatored<object>
            {
                Status = "error",
                Messages = new List<string> { context.Exception.Message }
            }
            )
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;

    }
}

   

