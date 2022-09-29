using Polly;
using Polly.Contrib.WaitAndRetry;
using Microsoft.Extensions.DependencyInjection;

using Coupa.Api;

const double EXPIRES_IN_20_HOURS = 60 /* seconds */ * 60 /* minutes */ * 20 /* hours */;

//I am just injecting these here for example. You should seed this from configuration or database store, etc.
var connectionSetting = new CoupaConnectionSetting(
    "replace_with_your_coupa_instance_hostname",                                                            //eg. awesomecompany-test.coupahost.com
    "replace_with_your_registered_oauth_client_Identifier",                                                 //example of how to find these values can be found in https://success.coupa.com/Integrate/KB/OAuth_2.0_Getting_Started_with_Coupa_API
    "replace_with_your_registered_oauth_client_Secret",                                                     //example of how to find these values can be found in https://success.coupa.com/Integrate/KB/OAuth_2.0_Getting_Started_with_Coupa_API
    "core.accounting.read core.common.read core.invoice.create core.invoice.read email login openid"       //Example scopes/claims required for your application, adjust as needed.
);

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var provider = SetupDependencyInjectionProvider(connectionSetting);
var coupaApiClient = GetCoupaApiClient(provider, connectionSetting);

var invoiceJson = await coupaApiClient.GetInvoiceJson(684, cancellationToken);                              //This is just an example of calling the api/invoices/684 where 684 is the invoice id in your Coupa instance
Console.WriteLine(invoiceJson);

static CoupaApiClient GetCoupaApiClient(IServiceProvider provider, CoupaConnectionSetting connectionSetting)
{
    return new CoupaApiClient(
        provider.GetRequiredService<IHttpClientFactory>(), 
        provider.GetRequiredService<ITokenStore>(), 
        connectionSetting,
        accessTokenExpiryOverride: EXPIRES_IN_20_HOURS);                                                    //Example on how to force 20 hours token expiry time. Since we are using a cache store, we can set this to the cache item expiry
                                                                                                            //If you are using some other cache store, you might want to implement it so it will delete and return no token when the token has expired
                                                                                                            //and force get a new access token from Core again.
                                                                                                            //the accessTokenExpireOverride param is optional.  If it is not there, it will default to the access token expiry_in that it got back
                                                                                                            //from when we request the token from Core instance which is defaulted to 1 full day (60 * 60 * 24 seconds) I think.


}

static IServiceProvider SetupDependencyInjectionProvider(CoupaConnectionSetting connectionSetting)
{
    var services  = new ServiceCollection()
        .AddSingleton<ITokenStore, MemoryCacheTokenStore>();                                                //We'll be using a sample in memory cache to store the OAuth token, replace with your own implemention, e.g. SqlServerTokenStore, etc.

    //Setup HttpClientFactory to create or return an http client for your Coupa Core instance and make sure it is resilience (retry communication upon transient Http failures, around 1 second each up to 3 times)
    //This is just an example, you can use a different retry strategy if you like.
    services
        .AddHttpClient(connectionSetting.HostName, httpClient =>{
            httpClient.BaseAddress = new Uri($"https://{connectionSetting.HostName}/");
        })
        .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3)));

    return services.BuildServiceProvider();
}
