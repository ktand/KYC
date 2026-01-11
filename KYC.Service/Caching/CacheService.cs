using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace KYC.Service.Caching;

/// <summary>
/// Provides caching functionality for storing and retrieving objects of type T using a distributed cache
/// implementation. Supports setting custom expiration and enforcing refresh logic. Multiple cache services can
/// use the same distributed cache instance as each type is prefixed with the type name.
/// </summary>
/// <typeparam name="T">The type of objects to be stored in the cache.</typeparam>
public class CacheService<T>(IDistributedCache cache, ILogger<CacheService<T>> logger) : ICacheService<T> where T : class?
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General);
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    private const string KeySeparator = ":";

    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromDays(1);

    public async Task<T> GetOrAddAsync(string key, Func<Task<T>> addItemFactory, TimeSpan? expiry = null, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var actualExpiry = expiry ?? DefaultExpiry;
        
        if (!forceRefresh && actualExpiry != TimeSpan.Zero)
        {
            var cachedValue = await GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (cachedValue != null) return cachedValue;
        }

        // Synchronize access to prevent multiple concurrent requests from hammering 
        // the data source. Only the first thread fetches data and subsequent threads 
        // will find the value in the cache after the lock is acquired.
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!forceRefresh && actualExpiry > TimeSpan.Zero)
            {
                var cachedValue = await GetAsync(key, cancellationToken).ConfigureAwait(false);
                if (cachedValue != null) return cachedValue;
            }

            logger.LogInformation("Cache miss for key '{Key}'. Fetching fresh data.", GetFullKey(key));
            
            var value = await addItemFactory().ConfigureAwait(false);
            
            if (value != null && actualExpiry > TimeSpan.Zero)
            {
                await SetAsync(key, value, actualExpiry, ct: cancellationToken).ConfigureAwait(false);
            }
            
            return value;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<T?> GetAsync(string key, CancellationToken ct = default)
    {
        ValidateKey(key);
        var fullKey = GetFullKey(key);
        
        var bytes = await cache.GetAsync(fullKey, ct).ConfigureAwait(false);
        if (bytes is null || bytes.Length == 0) return default;

        try
        {
            var envelope = JsonSerializer.Deserialize<CacheEntry>(bytes, _jsonOptions);
            return envelope is null 
                ? default 
                : envelope.Payload;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize cache entry for key '{Key}'. Removing entry.", key);
            await cache.RemoveAsync(fullKey, ct).ConfigureAwait(false);
            return default;
        }
    }

    private async Task SetAsync(string key, T? value, TimeSpan expiry, DistributedCacheEntryOptions? options = null, CancellationToken ct = default)
    {
        ValidateKey(key);
        
        var fullKey = GetFullKey(key);

        if (value is null)
        {
            // Don't cache null values
            return;
        }

        var effectiveOptions = options ?? new DistributedCacheEntryOptions();
        effectiveOptions.AbsoluteExpirationRelativeToNow = expiry;
        
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new CacheEntry(value), _jsonOptions);
        await cache.SetAsync(fullKey, bytes, effectiveOptions, ct).ConfigureAwait(false);
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
    }
    
    // Full key is built using typename + key separator + key
    private static string GetFullKey(string key) => $"{typeof(T).FullName}{KeySeparator}{key}";

    private sealed record CacheEntry(T? Payload);
}