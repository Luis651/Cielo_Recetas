using Supabase;
using Supabase.Gotrue;

namespace DulceRecetario.Shared.Services;

public class AuthService
{
    private readonly SupabaseService _supabaseService;

    /// <summary>Notifica cuando el estado de autenticación cambia (login/logout).</summary>
    public event Action? OnAuthStateChanged;

    public AuthService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    // ── Propiedades públicas ────────────────────────────────────────────────

    public bool IsAuthenticated => CurrentUser is not null;

    public User? CurrentUser
    {
        get
        {
            var client = _supabaseService.Client;
            return client?.Auth.CurrentUser;
        }
    }

    public string? CurrentUserId => CurrentUser?.Id;

    public string? CurrentUserEmail => CurrentUser?.Email;

    // ── Operaciones de Auth ─────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> SignInAsync(string email, string password)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var session = await client.Auth.SignIn(email, password);
            if (session?.User is null)
                return (false, "No se pudo iniciar sesión. Verifica tus datos.");

            OnAuthStateChanged?.Invoke();
            return (true, null);
        }
        catch (Exception ex)
        {
            var msg = ex.Message.Contains("Invalid login credentials")
                ? "Correo o contraseña incorrectos."
                : ex.Message.Contains("Email not confirmed")
                    ? "Debes confirmar tu correo electrónico antes de iniciar sesión."
                    : "Error al iniciar sesión. Intenta de nuevo.";
            return (false, msg);
        }
    }

    public async Task<(bool Success, string? Error)> SignUpAsync(string email, string password)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var session = await client.Auth.SignUp(email, password);

            // Supabase puede requerir confirmación de email según la configuración del proyecto
            if (session?.User is null)
                return (false, "No se pudo crear la cuenta. Intenta de nuevo.");

            // Si la sesión está activa directamente (auto-confirm activado), notificar
            if (session.User.ConfirmedAt.HasValue)
                OnAuthStateChanged?.Invoke();

            return (true, null);
        }
        catch (Exception ex)
        {
            var msg = ex.Message.Contains("already registered")
                ? "Este correo ya está registrado. Intenta iniciar sesión."
                : ex.Message.Contains("Password should be at least")
                    ? "La contraseña debe tener al menos 6 caracteres."
                    : "Error al crear la cuenta. Intenta de nuevo.";
            return (false, msg);
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            await client.Auth.SignOut();
            OnAuthStateChanged?.Invoke();
        }
        catch
        {
            // Limpiar estado local aunque falle el request
            OnAuthStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Restaura la sesión guardada en memoria si el cliente la guardó (AutoRefreshToken = true).
    /// Llamar al inicio de la app.
    /// </summary>
    public async Task TryRestoreSessionAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            // El cliente de Supabase con AutoRefreshToken recupera la sesión automáticamente
            // Si hay un usuario activo, disparamos el evento
            if (client.Auth.CurrentUser is not null)
                OnAuthStateChanged?.Invoke();
        }
        catch
        {
            // Sin sesión previa — no hay acción necesaria
        }
    }
}
