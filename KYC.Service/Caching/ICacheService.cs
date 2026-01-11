namespace KYC.Service.Caching;

public interface ICacheService<T>
{
    /// <summary>
    /// Get a value from the cache. If the key doesn't exist, the addItemFactory function will be called to get the
    /// value and store it in the cache.
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="addItemFactory">Function to get the key value if the entry is missing</param>
    /// <param name="expiry">Cache expiration timespan. Null = Use default expiry, TimeSpan.Zero = No caching</param>
    /// <param name="forceRefresh">Force refresh of the cached value, regardless of expiration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value</returns>
    Task<T> GetOrAddAsync(string key, Func<Task<T>> addItemFactory, TimeSpan? expiry = null,  bool forceRefresh = false, CancellationToken cancellationToken = default);
}