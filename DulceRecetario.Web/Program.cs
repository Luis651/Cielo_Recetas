using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DulceRecetario.Web;
using DulceRecetario.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Reutilizamos los servicios de Supabase
builder.Services.AddSingleton<SupabaseService>();
builder.Services.AddScoped<RecipeService>();

await builder.Build().RunAsync();
