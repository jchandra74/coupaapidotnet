namespace Coupa.Api;

public record OAuthToken
{
    // Yes, yes, yes... This is not proper C# naming, but I am trying to appease the JSON god, so let it be.
    public string? access_token { get; set; }
    public string? token_type { get; set; }
    public double expires_in { get; set; }

    public static OAuthToken Null => new OAuthToken { access_token = "", token_type = "", expires_in = 0 };
}