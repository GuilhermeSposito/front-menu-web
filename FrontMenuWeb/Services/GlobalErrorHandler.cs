using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using MudBlazor;
namespace FrontMenuWeb.Services;

public class GlobalErrorHandler : IErrorBoundaryLogger
{
    private readonly ISnackbar _snackbar;

    public GlobalErrorHandler(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    public ValueTask LogErrorAsync(Exception exception)
    {
        // Mostra um snackbar pro usuário
        _snackbar.Add($"Ocorreu um erro inesperado: {exception.Message}", Severity.Error, config =>
        {
            config.RequireInteraction = false;
            config.VisibleStateDuration = 6000;
        });

        // Opcional: logar no console também
        Console.Error.WriteLine($"Erro global: {exception}");

        return ValueTask.CompletedTask;
    }
}
