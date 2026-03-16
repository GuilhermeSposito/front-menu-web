using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;
using Unimake.Business.DFe.Xml.DARE;

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
                    [{DateTime.Now.AddHours(-3):dd/MM/yyy}] {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}

                    Status: 500
                    Mensagem: {ex.Message}

                    Stack: {ex.StackTrace}
                    """;

                var html = $"""
                <div style="font-family: Arial, sans-serif; background-color:#f4f4f4; padding:20px;">
        
                    <div style="max-width:800px; margin:auto; background:white; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.1);">
            
                        <div style="background-color:#c62828; color:white; padding:15px;">
                            <h2 style="margin:0;">🚨 Erro na API De Integrações</h2>
                        </div>

                        <div style="padding:20px; color:#333;">
                
                            <p><strong>Data:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                            <p><strong>Servidor:</strong> {Environment.MachineName}</p>

                            <hr style="margin:20px 0;" />

                            <h3 style="color:#c62828;">Detalhes do Erro</h3>

                            <pre style="
                                background:#1e1e1e;
                                color:#FFFFFF;
                                padding:15px;
                                border-radius:5px;
                                overflow:auto;
                                font-size:13px;
                            ">

                            {logFormatado}
                            </pre>

                        </div>
                    </div>
                </div>
                """;


                await emailService.EnviarAsync(
                    "guilherme@sophos-erp.com.br", 
                    $"Log De Erro Na APi {DateTime.Now:g}",
                    html);


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

   

