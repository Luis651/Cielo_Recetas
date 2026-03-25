using Supabase.Gotrue;
using System.Threading.Tasks;
using DulceRecetario.Shared.Services;

namespace DulceRecetario.Services;

public class AuthService
{
    private readonly SupabaseService _supabaseService;

    public AuthService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<Session?> SignUpAsync(string email, string password)
    {
        var client = await _supabaseService.GetClientAsync();
        return await client.Auth.SignUp(email, password);
    }

    public async Task<Session?> SignInAsync(string email, string password)
    {
        var client = await _supabaseService.GetClientAsync();
        return await client.Auth.SignIn(email, password);
    }

    public async Task SignOutAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        await client.Auth.SignOut();
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        return client.Auth.CurrentUser;
    }
}
