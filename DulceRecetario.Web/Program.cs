using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using DulceRecetario.Web;
using DulceRecetario.Web.Auth;
using DulceRecetario.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ── Servicios de Supabase ─────────────────────────────────
builder.Services.AddSingleton<SupabaseService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<RecipeService>();

// ── Autenticación de Blazor ───────────────────────────────
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CustomAuthStateProvider>());

// ── Restaurar sesión al arrancar ──────────────────────────
var host = builder.Build();
var authService = host.Services.GetRequiredService<AuthService>();
await authService.TryRestoreSessionAsync();

await host.RunAsync();
