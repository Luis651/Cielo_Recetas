using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using DulceRecetario.Shared.Services;

namespace DulceRecetario.Web.Auth;

/// <summary>
/// Puente entre AuthService (Supabase) y el sistema de autenticación de Blazor.
/// Blazor lo usa para saber si mostrar rutas protegidas y quién es el usuario.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    public CustomAuthStateProvider(AuthService authService)
    {
        _authService = authService;
        // Escuchar los cambios de login/logout y notificar a Blazor
        _authService.OnAuthStateChanged += NotifyStateChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authService.IsAuthenticated && _authService.CurrentUser is not null)
        {
            var user = _authService.CurrentUser;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
            };
            var identity = new ClaimsIdentity(claims, "Supabase");
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(principal));
        }

        // Usuario anónimo
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    private void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
