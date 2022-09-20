namespace Coupa.Api;

public class CoupaConnectionSetting
{
    public string HostName { get; }
    public string ClientId { get; }
    public string ClientSecret { get; }
    public string Scopes { get; }

    public CoupaConnectionSetting(string hostName, string clientId, string clientSecret, string scopes)
    {
        HostName = hostName;
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scopes = scopes;
    }
}