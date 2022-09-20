# .NET 6 Example of Resilient OAuth2 Enabled Coupa API Client Implementation

## Overview
---
This example will show you how you can implement a Coupa API Client that communicates with your Coupa Core instance using OAuth2 Authorization.

The sample is setup to implement a `MemoryCache` OAuth Access Token store so it does not have to ask for access token all the time from your Coupa Core instance.  This implementation of `MemoryCacheTokenStore` will expire a cached access token after a certain time.  In this example, I am showing you how to expire the access token from cache after 20 hours.  See line `32` in `Program.cs` for how to do this.

The sample is also setup to use the `Polly` NuGet package and other related helper NuGet packages such as `Microsoft.Extensions.Http.Polly` and `Polly.Contrib.WaitAndRetry` to allow communication resiliency upon transient HTTP error/issue.   See line `52` in `Program.cs` for how to do this.

## Requirements
---
To run this sample, you need **.NET 6** or later SDK to be installed.  If you are using lower version of .NET, you need to change the `.csproj` and `Program.cs` to not use the top level statements. See https://learn.microsoft.com/en-us/dotnet/core/tutorials/top-level-templates for details.  You can download **.NET 6 SDK** from https://dotnet.microsoft.com/en-us/download.

## Setup
---
Follow the example on https://success.coupa.com/Integrate/KB/OAuth_2.0_Getting_Started_with_Coupa_API on how to setup your OAuth Client entry in your Coupa Core instance.

**IMPORTANT!!**  You might encounter issue when creating your OAuth Client when following the tutorial in the **Coupa Success** article above.  If you can't see the **Secret** field, you probably entered the **Jwks Uri** when setting up the OAuth Client entry.  If so, just create another Oauth Client entry but this time, **DO NOT** enter anything in the **Jwks Uri** field.

Once you do that, take note (copy & paste) the **Identifier** and **Secret** from your Oauth Client entry and paste them into the appropriate places in `Program.cs` line `12` and `13`.

Also, replace line `11` in `Program.cs` with your Coupa Core instance hostname

Ensure that the `Oidc Scopes` that you assigned to the OAuth Client entry are also copied (after removing all the commas) into line `14` in `Program.cs`.

In this sample, I am calling the `/api/invoices/{id}` where the `id` is an id of a particular invoice that could be found in the Coupa Core instance.  In this case, it is important to have the `core.invoice.read` **Oidc Scope** assigned since we are going to read the invoice from that Core instance.

You will want to replace `684` in line `23` of `Program.cs` with a valid invoice Id on your Coupa Core instance where you know that you have read access to.

## Running the Sample
---
Once all of these are done, you can run this sample by typing `dotnet run` from your terminal/command prompt/bash shell, etc.

When it runs, it should:
1. Connect to the Core instance and fetch an OAuth access token, cache it in the `MemoryCacheTokenStore`
2. Fetch the invoice with the specific invoice id and display the `JSON` string of that retrieved invoice.