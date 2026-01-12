# Othoba.BanglaLinkOrange.Client

A convenience library providing factory patterns and extension methods for simplified Banglalink OAuth 2.0 client creation and configuration.

## Supported Frameworks

- **.NET 6.0** - Uses Microsoft.Extensions.* v6.0.0 packages
- **.NET 8.0** - Uses Microsoft.Extensions.* v8.0.0 packages

## Features

- **Fluent Configuration API** - Easy, chainable client setup
- **Factory Pattern** - Simplified client instantiation
- **Extension Methods** - Quick helper methods for common scenarios
- **Multi-Framework Support** - Framework-specific package versions automatically selected

## Usage

### Using the Factory

```csharp
using Othoba.BanglaLinkOrangeClient.Client;

// Create a client using the factory
var client = new BanglalinkOAuthClientFactory()
    .WithBaseUrl("http://1.2.3.4:8080")
    .WithClientCredentials("client-id", "client-secret")
    .WithUserCredentials("username", "password")
    .WithScope("openid")
    .WithAutoRefreshToken(true)
    .Build();

// Use the client
var token = await client.GetValidAccessTokenAsync();
```

### Using Quick Creation

```csharp
using Othoba.BanglaLinkOrangeClient.Client;

var client = ClientExtensions.CreateQuickClient(
    baseUrl: "http://1.2.3.4:8080",
    clientId: "client-id",
    clientSecret: "client-secret",
    username: "username",
    password: "password"
);

var token = await client.GetValidAccessTokenAsync();
```

### Using Custom Service Collection

```csharp
var services = new ServiceCollection();
// Add other services as needed...

var client = new BanglalinkOAuthClientFactory(services)
    .WithBaseUrl("http://1.2.3.4:8080")
    .WithClientCredentials("client-id", "client-secret")
    .WithUserCredentials("username", "password")
    .Build();
```

## Configuration Options

- `WithBaseUrl(string)` - Sets the OAuth token endpoint base URL
- `WithClientCredentials(string, string)` - Sets client ID and secret
- `WithUserCredentials(string, string)` - Sets username and password
- `WithScope(string)` - Sets OAuth scope (default: "openid")
- `WithHttpClientTimeout(TimeSpan)` - Sets HTTP timeout (default: 30 seconds)
- `WithAutoRefreshToken(bool)` - Enables automatic token refresh (default: true)

## Build Behavior

The client project automatically uses the correct package versions based on the target framework:

- **net6.0**: Microsoft.Extensions.Http 6.0.0, Microsoft.Extensions.Http.Polly 6.0.0
- **net8.0**: Microsoft.Extensions.Http 8.0.0, Microsoft.Extensions.Http.Polly 8.0.0

Polly (resilience library) version 8.2.0 is used for both frameworks.

## Error Handling

Configuration validation occurs during build:

```csharp
try
{
    var client = factory.Build();
}
catch (InvalidOperationException ex)
{
    // Configuration validation failed
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

## Related

- [Othoba.BanglaLinkOrange](../Othoba.BanglaLinkOrange/README.md) - Core OAuth client library
- [Examples](../../examples/) - Usage examples for .NET 6 and .NET 8
