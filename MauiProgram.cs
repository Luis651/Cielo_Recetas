using Microsoft.Extensions.Logging;
using DulceRecetario.Services;
using DulceRecetario.ViewModels;
using DulceRecetario.Views;

namespace DulceRecetario;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Servicios ────────────────────────────────────────────────────
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddTransient<RecipeService>();

        // ── ViewModels ───────────────────────────────────────────────────
        builder.Services.AddTransient<RecipeListViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<RecipeFormViewModel>();
        builder.Services.AddTransient<AuthViewModel>();

        // ── Pages ────────────────────────────────────────────────────────
        builder.Services.AddTransient<RecipeListPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<RecipeFormPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
