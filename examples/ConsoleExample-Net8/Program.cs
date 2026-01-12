using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Exceptions;

// Example 1: Basic Usage with Dependency Injection
Console.WriteLine("=== Othoba Banglalink OAuth 2.0 Client - Console Example (.NET 8) ===\n");

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Option A: Configure from appsettings.json
var banglalinkConfig = new BanglalinkClientConfiguration();
configuration.GetSection("BanglalinkOAuth").Bind(banglalinkConfig);

// Option B: Configure programmatically (uncomment to use instead)
// services.AddBanglalinkAuthClient(config =>
// {
//     config.BaseUrl = "http://1.2.3.4:8080";
//     config.ClientId = "your-client-id";
//     config.ClientSecret = "your-client-secret";
//     config.Username = "your-username";
//     config.Password = "your-password";
// });

// Register the client
services.AddBanglalinkAuthClient(banglalinkConfig);

var serviceProvider = services.BuildServiceProvider();
var authClient = serviceProvider.GetRequiredService<IBanglalinkAuthClient>();

try
{
    Console.WriteLine("Attempting to authenticate with Banglalink...\n");

    // Get a valid access token (will authenticate if not cached)
    var token = await authClient.GetValidAccessTokenAsync();
    Console.WriteLine($"✓ Access Token obtained successfully");
    Console.WriteLine($"  Token (first 50 chars): {token[..50]}...");
    Console.WriteLine();

    // Get the full token response
    var tokenResponse = await authClient.GetValidTokenResponseAsync();
    Console.WriteLine($"Token Response Details:");
    Console.WriteLine($"  - Token Type: {tokenResponse.TokenType}");
    Console.WriteLine($"  - Expires In: {tokenResponse.ExpiresIn} seconds");
    Console.WriteLine($"  - Expires At: {tokenResponse.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"  - Refresh Token Expires At: {tokenResponse.RefreshExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"  - Is Access Token Valid: {tokenResponse.IsAccessTokenValid}");
    Console.WriteLine($"  - Is Refresh Token Valid: {tokenResponse.IsRefreshTokenValid}");
    Console.WriteLine();

    // Example: Check cached token
    var cachedToken = authClient.GetCachedTokenResponse();
    if (cachedToken != null)
    {
        Console.WriteLine($"✓ Token is cached in memory");
        Console.WriteLine($"  Time remaining: {(cachedToken.ExpiresAt - DateTime.UtcNow).TotalSeconds:F0} seconds");
        Console.WriteLine();
    }

    // Example: Get token again (will use cached version)
    Console.WriteLine("Getting token again (should use cached version)...");
    var token2 = await authClient.GetValidAccessTokenAsync();
    Console.WriteLine($"✓ Token retrieved (same as cached: {token == token2})");
    Console.WriteLine();

    // Example: Clear cache and re-authenticate
    Console.WriteLine("Clearing cache...");
    authClient.ClearCache();
    Console.WriteLine($"✓ Cache cleared");
    Console.WriteLine();

    // Example: Manual refresh
    Console.WriteLine("Refreshing token manually...");
    var refreshToken = tokenResponse.RefreshToken;
    var newTokenResponse = await authClient.RefreshTokenAsync(refreshToken);
    Console.WriteLine($"✓ Token refreshed successfully");
    Console.WriteLine($"  New token expires in: {newTokenResponse.ExpiresIn} seconds");
    Console.WriteLine();

    Console.WriteLine("=== All operations completed successfully! ===");
}
catch (BanglalinkAuthenticationException ex)
{
    Console.WriteLine($"✗ Authentication Failed: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"  Inner error: {ex.InnerException.Message}");
    }
}
catch (BanglalinkConfigurationException ex)
{
    Console.WriteLine($"✗ Configuration Error: {ex.Message}");
    Console.WriteLine($"\nPlease ensure appsettings.json contains:");
    Console.WriteLine(@"
{
  ""BanglalinkOAuth"": {
    ""BaseUrl"": ""http://IP:Port"",
    ""ClientId"": ""your-client-id"",
    ""ClientSecret"": ""your-client-secret"",
    ""Username"": ""your-username"",
    ""Password"": ""your-password""
  }
}
");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Unexpected Error: {ex.Message}");
    Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
}
