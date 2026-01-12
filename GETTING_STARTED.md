# Getting Started with Othoba.BanglaLinkOrangeClient

This guide will help you get started with the Othoba Banglalink OAuth 2.0 Client library.

## Prerequisites

- .NET 6.0 or .NET 8.0 (or later)
- Visual Studio 2022 (optional but recommended)
- Banglalink API credentials (provided by Banglalink):
  - Base URL (IP:Port)
  - Client ID
  - Client Secret
  - Username
  - Password

## Installation

### Via NuGet Package Manager

```bash
dotnet add package Othoba.BanglaLinkOrangeClient
```

### Via Package Manager Console

```powershell
Install-Package Othoba.BanglaLinkOrangeClient
```

## Basic Setup

### 1. Console Application Example

Create a new console application:

```bash
dotnet new console -n BanglalinkExample
cd BanglalinkExample
dotnet add package Othoba.BanglaLinkOrangeClient
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Configuration.Json
```

Create `appsettings.json`:

```json
{
  "BanglalinkOAuth": {
    "BaseUrl": "http://1.2.3.4:8080",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Username": "your-username",
    "Password": "your-password"
  }
}
```

Update `Program.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange;
using Othoba.BanglaLinkOrange.Clients;

var services = new ServiceCollection();
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

services.AddBanglalinkAuthClient(cfg =>
{
    var section = config.GetSection("BanglalinkOAuth");
    cfg.BaseUrl = section["BaseUrl"]!;
    cfg.ClientId = section["ClientId"]!;
    cfg.ClientSecret = section["ClientSecret"]!;
    cfg.Username = section["Username"]!;
    cfg.Password = section["Password"]!;
});

var provider = services.BuildServiceProvider();
var authClient = provider.GetRequiredService<IBanglalinkAuthClient>();

// Get access token
var token = await authClient.GetValidAccessTokenAsync();
Console.WriteLine($"Access Token: {token}");
```

Run:

```bash
dotnet run
```

### 2. ASP.NET Core Web API Example

Create a new Web API project:

```bash
dotnet new webapi -n BanglalinkApi
cd BanglalinkApi
dotnet add package Othoba.BanglaLinkOrangeClient
```

Update `Program.cs`:

```csharp
using Othoba.BanglaLinkOrangeClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Register Banglalink client
builder.Services.AddBanglalinkAuthClient(config =>
{
    var section = builder.Configuration.GetSection("BanglalinkOAuth");
    config.BaseUrl = section["BaseUrl"] ?? throw new InvalidOperationException("BaseUrl required");
    config.ClientId = section["ClientId"] ?? throw new InvalidOperationException("ClientId required");
    config.ClientSecret = section["ClientSecret"] ?? throw new InvalidOperationException("ClientSecret required");
    config.Username = section["Username"] ?? throw new InvalidOperationException("Username required");
    config.Password = section["Password"] ?? throw new InvalidOperationException("Password required");
});

var app = builder.Build();
app.UseSwagger();
app.MapControllers();
app.Run();
```

Create `Controllers/TokenController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Othoba.BanglaLinkOrangeClient.Clients;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IBanglalinkAuthClient _authClient;

    public TokenController(IBanglalinkAuthClient authClient)
    {
        _authClient = authClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetToken()
    {
        var token = await _authClient.GetValidAccessTokenAsync();
        return Ok(new { access_token = token, token_type = "Bearer" });
    }
}
```

## Common Tasks

### Getting an Access Token

```csharp
var token = await authClient.GetValidAccessTokenAsync();
// Token is cached and automatically refreshed when expired
```

### Using Token with HttpClient

```csharp
var token = await authClient.GetValidAccessTokenAsync();
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
request.Headers.Authorization = new("Bearer", token);
var response = await httpClient.SendAsync(request);
```

### Checking Token Status

```csharp
var tokenResponse = await authClient.GetValidTokenResponseAsync();

if (tokenResponse.IsAccessTokenValid)
{
    Console.WriteLine($"Token valid until: {tokenResponse.ExpiresAt}");
}
else
{
    Console.WriteLine("Token expired");
}
```

### Clearing Cached Token

```csharp
authClient.ClearCache();
// Next call to GetValidAccessTokenAsync() will authenticate fresh
```

## Error Handling

```csharp
try
{
    var token = await authClient.GetValidAccessTokenAsync();
}
catch (BanglalinkAuthenticationException ex)
{
    Console.WriteLine($"Auth failed: {ex.Message}");
}
catch (BanglalinkConfigurationException ex)
{
    Console.WriteLine($"Config error: {ex.Message}");
}
catch (BanglalinkClientException ex)
{
    Console.WriteLine($"Client error: {ex.Message}");
}
```

## Testing

Create a test project:

```bash
dotnet new xunit -n BanglalinkClient.Tests
cd BanglalinkClient.Tests
dotnet add package Othoba.BanglaLinkOrangeClient
dotnet add package Moq
```

Example test:

```csharp
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Clients;
using Xunit;

public class BanglalinkAuthClientTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "test-id",
            ClientSecret = "test-secret",
            Username = "user",
            Password = "pass"
        };

        Assert.True(config.IsValid());
    }

    [Fact]
    public void Configuration_InvalidWithoutBaseUrl()
    {
        var config = new BanglalinkClientConfiguration
        {
            ClientId = "test-id",
            ClientSecret = "test-secret",
            Username = "user",
            Password = "pass"
        };

        Assert.False(config.IsValid());
    }
}
```

## Troubleshooting

### "Configuration is invalid"
Ensure all required settings are present in `appsettings.json`:
- BaseUrl
- ClientId
- ClientSecret
- Username
- Password

### "Authentication failed"
- Verify credentials with Banglalink
- Check that the BaseUrl is correct
- Ensure network connectivity to the server

### "Token expired"
The library automatically handles token refresh. If you need manual control:

```csharp
authClient.ClearCache();
var newToken = await authClient.GetValidAccessTokenAsync();
```

## Next Steps

1. Check the [README.md](../README.md) for detailed API documentation
2. Review the [examples](../examples) folder for more use cases
3. Explore the source code in the `src` folder

## Support

For issues:
1. Check the [troubleshooting section](#troubleshooting)
2. Review the API specification documentation provided by Banglalink
3. Create an issue in the repository with detailed error information
