using System.Text.Json;
using System.Net.Http.Headers;

namespace Coupa.Api;

public class CoupaApiClient
{
    /* 
        This is just an example implementation of a Coupa API Client.
        I only implemented a couple of API calls for example:
            - GetInvoiceJson
            - GetInboundInvoicesJson
    */
    private readonly HttpClient _client;
    private readonly ITokenStore _store;
    private readonly CoupaConnectionSetting _setting;

    private readonly double? _accessTokenExpiryOverride;


    public CoupaApiClient(IHttpClientFactory factory, ITokenStore store, CoupaConnectionSetting setting, double? accessTokenExpiryOverride = default)
    {
        _store = store;
        _setting = setting;
        _accessTokenExpiryOverride = accessTokenExpiryOverride;
        _client = factory.CreateClient(_setting.HostName);
    }

    public async Task<string> GetInvoiceJson(long invoiceId, CancellationToken cancellationToken = default)
    {
        _ = invoiceId > 0 ? 0 : throw new ArgumentOutOfRangeException(nameof(invoiceId));

        await EnsureOAuthTokenAsync(cancellationToken).ConfigureAwait(false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/invoices/{invoiceId}");
        request.Headers.Remove("Accept");
        request.Headers.TryAddWithoutValidation("Accept", "application/json");

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetInboundInvoicesJson(CancellationToken cancellationToken = default)
    {
        await EnsureOAuthTokenAsync(cancellationToken).ConfigureAwait(false);

        var request = new HttpRequestMessage(HttpMethod.Get, "api/inbound_invoices");
        request.Headers.Remove("Accept");
        request.Headers.TryAddWithoutValidation("Accept", "application/json");

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureOAuthTokenAsync(CancellationToken cancellationToken)
    {
        OAuthToken token;
        bool tokenRetrieved;

        (tokenRetrieved, token) = await TryFetchOAuthTokenFromStore(cancellationToken).ConfigureAwait(false);
        if (tokenRetrieved)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            return;
        }

        (tokenRetrieved, token) = await TryFetchOAuthTokenFromCoupaInstance(cancellationToken).ConfigureAwait(false);
        if (tokenRetrieved)
        {
            await _store.PutTokenAsync(_setting.HostName, token, _accessTokenExpiryOverride ?? token.expires_in).ConfigureAwait(false);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
        }
    }

    private async Task<(bool tokenRetrieved, OAuthToken token)> TryFetchOAuthTokenFromStore(CancellationToken cancellationToken)
    {
        var token = await _store.GetTokenAsync(_setting.HostName, cancellationToken).ConfigureAwait(false);
        return (token != default, token ?? OAuthToken.Null);
    }

    private async Task<(bool tokenRetrieved, OAuthToken token)> TryFetchOAuthTokenFromCoupaInstance(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token");
        request.Content = new FormUrlEncodedContent(new [] {
            new KeyValuePair<string, string>("client_id", _setting.ClientId),
            new KeyValuePair<string, string>("client_secret", _setting.ClientSecret),
            new KeyValuePair<string, string>("scope", _setting.Scopes),
            new KeyValuePair<string, string>("grant_type","client_credentials")
        });

        try
        {
            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var token = JsonSerializer.Deserialize<OAuthToken>(raw);
            if (token == default)
                throw new Exception("Unable to retrieve a Coupa Access Token.");

            return (token != default, token ?? OAuthToken.Null);
                
        }
        catch(Exception ex)
        {
            throw new Exception("Unable to retrieve a Coupa Access Token.", ex);
        }
    }
}