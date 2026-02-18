using FrontMenuWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontMenuWeb.Services;

public interface ILogoutService
{
    Task ForceLogout();
}
public class LogoutService : ILogoutService
{
    private readonly NavigationManager _navigation;
    private readonly AuthenticationStateProvider _auth;

    public LogoutService(NavigationManager navigation, AuthenticationStateProvider auth)
    {
        _navigation = navigation;
        _auth = auth;
    }

    public Task ForceLogout()
    {
        if (_auth is CustomAuthStateProvider custom)
            custom.NotifyAuthenticationStateChanged();

        _navigation.NavigateTo("/login", forceLoad: true);

        return Task.CompletedTask;
    }
}
