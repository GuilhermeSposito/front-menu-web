using MudBlazor;
using FrontMenuWeb.Components.Modais.ModaisGenericos;

namespace FrontMenuWeb.Services;

public class AuthorizationService
{
    private readonly MerchantServices _merchantServices;
    private readonly IDialogService _dialogService;

    public AuthorizationService(MerchantServices merchantServices, IDialogService dialogService)
    {
        _merchantServices = merchantServices;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Valida uma permissão chamando o backend. Se não houver permissão, abre o modal de autorização de supervisor.
    /// </summary>
    /// <param name="permissionName">Nome da permissão a ser validada.</param>
    /// <param name="title">Título do modal de autorização.</param>
    /// <returns>True se autorizado (pelo usuário ou supervisor), False caso contrário.</returns>
    public async Task<bool> ValidateAsync(string permissionName, string title = "Autorização de Supervisor")
    {
        // 1. Valida a permissão chamando o backend (ou retorna true se for proprietário)
        bool temPermissao = await _merchantServices.CheckPermissionAsync(permissionName);
        if (temPermissao) return true;

        // 2. Se não tem permissão, abre o modal de login temporário (Supervisor)
        var parameters = new DialogParameters { ["PermissionName"] = permissionName };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        
        var dialogReferencia = await _dialogService.ShowAsync<ModalDeLoginTemporario>(title, parameters, options);
        var retornoDialogo = await dialogReferencia.Result;

        if (retornoDialogo != null && !retornoDialogo.Canceled && retornoDialogo.Data is bool autorizado)
        {
            return autorizado;
        }

        return false;
    }
}
