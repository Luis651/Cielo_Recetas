using Supabase;

namespace DulceRecetario.Shared.Services;

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
                if (string.IsNullOrWhiteSpace(SupabaseConfig.Url))
                    throw new InvalidOperationException("Supabase URL no configurada.");

                var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                };
                
                _client = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, options);
                await _client.InitializeAsync();
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
