using ApiFiscalMenuWeb.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception.ToString());

        int statusCode = context.Exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Result = new ObjectResult(
            new RetunApiRefatored
            {
                status = "error",
                message = new List<string> { context.Exception.Message }
            }
            )
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;

    }

    /*public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception.ToString());

        context.Result = new ObjectResult(context.Exception.Message) { StatusCode = StatusCodes.Status500InternalServerError };
    }*/
}

