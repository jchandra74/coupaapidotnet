namespace Coupa.Api;

public interface ITokenStore
{
    Task<OAuthToken> GetTokenAsync(string hostName, CancellationToken cancellationToken = default);
    Task PutTokenAsync(string hostName, OAuthToken token, double? customExpirationSeconds = null, CancellationToken cancellationToken = default);

    Task RemoveTokenAsync(string hostName, CancellationToken cancellationToken = default);
}