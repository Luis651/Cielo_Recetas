using DulceRecetario.Views;
using DulceRecetario.Services;

namespace DulceRecetario;

public partial class AppShell : Shell
{
    private readonly AuthService _authService;

    public AppShell(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();

        // Registrar rutas de navegación push
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(RecipeFormPage), typeof(RecipeFormPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckSession();
    }

    private async Task CheckSession()
    {
        try 
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking session: {ex.Message}");
        }
    }
}
