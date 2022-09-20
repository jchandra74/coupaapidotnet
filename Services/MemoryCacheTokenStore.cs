using System.Runtime.Caching;

namespace Coupa.Api;

public class MemoryCacheTokenStore : ITokenStore
{
    private readonly MemoryCache _cache;

    public MemoryCacheTokenStore()
    {
        _cache = MemoryCache.Default;
    }

    public Task<OAuthToken> GetTokenAsync(string hostName, CancellationToken cancellationToken = default)
    {
        _ = hostName ?? throw new ArgumentNullException(nameof(hostName));

        var token = (OAuthToken)_cache.Get(hostName);
        return Task.FromResult(token);
    }

    public Task PutTokenAsync(string hostName, OAuthToken token, double? customExpirationSeconds = null, CancellationToken cancellationToken = default)
    {
        _ = hostName ?? throw new ArgumentNullException(nameof(hostName));
        _ = token ?? throw new ArgumentNullException(nameof(token));

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(customExpirationSeconds ?? token.expires_in);

        if (!_cache.Contains(hostName))
            _cache.Add(hostName, token, expiresAt);
        else
            _cache.Set(hostName, token, expiresAt);

        return Task.CompletedTask;
    }

    public Task RemoveTokenAsync(string hostName, CancellationToken cancellationToken = default)
    {
        if (_cache.Contains(hostName))
            _cache.Remove(hostName);

        return Task.CompletedTask;
    }
}