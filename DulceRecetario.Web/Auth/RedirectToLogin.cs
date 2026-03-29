// Componente helper: redirige a /login si el usuario no está autenticado
// Usado por AuthorizeRouteView en App.razor

using Microsoft.AspNetCore.Components;

namespace DulceRecetario.Web.Auth;

public class RedirectToLogin : ComponentBase
{
    [Inject] NavigationManager Navigation { get; set; } = null!;

    protected override void OnInitialized()
    {
        Navigation.NavigateTo("/login", replace: true);
    }
}
