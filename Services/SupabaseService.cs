using Supabase;

namespace DulceRecetario.Services;

/// <summary>
/// Proveedor del cliente Supabase (Singleton via DI).
/// </summary>
public class SupabaseService
{
    private Client? _client;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<Client> GetClientAsync()
    {
        if (_client is not null)
            return _client;

        await _semaphore.WaitAsync();
        try
        {
            if (_client is null)
            {
                if (string.IsNullOrWhiteSpace(SupabaseConfig.Url) || SupabaseConfig.Url.Contains("YOUR_PROJECT_REF"))
                    throw new InvalidOperationException("Supabase URL no configurada.");

                var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                };
                
                _client = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, options);
                
                try 
                {
                    await _client.InitializeAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error inicializando Supabase: {ex.Message}");
                    _client = null; // Reintenta luego
                    throw;
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return _client;
    }

    public Client? Client => _client;
}
