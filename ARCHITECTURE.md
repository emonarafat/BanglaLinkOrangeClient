# Architecture & Design

## Overview

The Othoba.BanglaLinkOrangeClient library is designed with a focus on:

- **Simplicity**: Easy to understand and use API
- **Reliability**: Thread-safe, automatic token management
- **Flexibility**: Works with DI containers or standalone
- **Standards**: Follows OAuth 2.0 specification
- **Best Practices**: Modern C# patterns and practices

## Directory Structure

```
Othoba.BanglaLinkOrangeClient/
├── src/
│   └── Othoba.BanglaLinkOrangeClient/
│       ├── Clients/                 # HTTP client implementations
│       │   └── BanglalinkAuthClient.cs
│       ├── Configuration/           # Configuration classes
│       │   └── BanglalinkClientConfiguration.cs
│       ├── Exceptions/              # Custom exceptions
│       │   └── BanglalinkClientException.cs
│       ├── Models/                  # Data models
│       │   └── BanglalinkTokenResponse.cs
│       ├── Utilities/               # Helper utilities
│       │   └── BasicAuthenticationGenerator.cs
│       ├── ServiceCollectionExtensions.cs
│       └── Othoba.BanglaLinkOrangeClient.csproj
├── examples/
│   ├── ConsoleExample/              # Console app example
│   └── WebApiExample/               # ASP.NET Core example
├── README.md
├── GETTING_STARTED.md
└── Othoba.BanglaLinkOrangeClient.sln (solution file references all projects)
```

## Core Components

### 1. Configuration (`BanglalinkClientConfiguration`)

Holds all settings needed to connect to Banglalink OAuth server.

**Responsibilities:**
- Store configuration values
- Validate configuration completeness
- Provide configuration errors

```csharp
var config = new BanglalinkClientConfiguration
{
    BaseUrl = "http://1.2.3.4:8080",
    ClientId = "client-id",
    ClientSecret = "client-secret",
    Username = "user",
    Password = "pass"
};

if (config.IsValid())
{
    var errors = config.GetValidationErrors();
}
```

### 2. Token Response Model (`BanglalinkTokenResponse`)

Represents the OAuth token response from the server.

**Features:**
- Automatic expiration calculation
- Token validity checking with buffer
- Supports both access and refresh tokens

```csharp
var response = new BanglalinkTokenResponse
{
    AccessToken = "...",
    ExpiresIn = 36000,
    RefreshToken = "...",
    RefreshExpiresIn = 36000
};

// Check if token is still valid (with 30-second buffer)
if (response.IsAccessTokenValid)
{
    // Use the token
}

// Get exact expiration time
var expiresAt = response.ExpiresAt; // DateTime
```

### 3. Authentication Client (`BanglalinkAuthClient`)

Main client for OAuth operations.

**Key Features:**
- Password Grant (username/password authentication)
- Refresh Token Grant (token renewal)
- Automatic token caching
- Thread-safe operations
- Automatic token refresh on demand

**Token Flow:**

```
User calls GetValidAccessTokenAsync()
         ↓
    Is cached token valid?
         ├─ YES → Return cached token
         └─ NO  → Is refresh token valid?
                  ├─ YES (AutoRefresh enabled) → RefreshTokenAsync()
                  └─ NO → AuthenticateAsync()
                          (Fresh login with username/password)
         ↓
    Cache and return token
```

### 4. Exceptions

Hierarchy:
```
BanglalinkClientException (base)
├── BanglalinkAuthenticationException
└── BanglalinkConfigurationException
```

### 5. Utilities

**`BasicAuthenticationGenerator`**
- Generates Base64-encoded credentials for Basic Auth header
- Used internally by the client

```csharp
var basicAuth = BasicAuthenticationGenerator.GenerateToken("client-id", "client-secret");
// Output: base64(client-id:client-secret)
```

## Design Patterns

### 1. Dependency Injection

The library is designed to work with .NET's built-in DI container:

```csharp
services.AddBanglalinkAuthClient(config => {
    config.BaseUrl = "...";
    // ...
});

var client = serviceProvider.GetRequiredService<IBanglalinkAuthClient>();
```

### 2. Interface-Based Design

All public APIs are behind interfaces for testability:

```csharp
public interface IBanglalinkAuthClient
{
    Task<string> GetValidAccessTokenAsync();
    Task<BanglalinkTokenResponse> GetValidTokenResponseAsync();
    // ...
}
```

### 3. Token Caching

Tokens are cached in memory with thread-safe access:

```csharp
private BanglalinkTokenResponse? _cachedTokenResponse;
private readonly object _lockObject = new object();

lock (_lockObject)
{
    if (_cachedTokenResponse?.IsAccessTokenValid == true)
    {
        return _cachedTokenResponse;
    }
}
```

### 4. Graceful Degradation

If auto-refresh is disabled, the client still works but requires manual token management.

## Thread Safety

The client is thread-safe for concurrent access:

```csharp
// Safe to call from multiple threads simultaneously
var task1 = Task.Run(() => authClient.GetValidAccessTokenAsync());
var task2 = Task.Run(() => authClient.GetValidAccessTokenAsync());
var task3 = Task.Run(() => authClient.RefreshTokenAsync(refreshToken));

await Task.WhenAll(task1, task2, task3);
```

All concurrent requests will:
1. Use cached token if valid
2. Queue refresh requests to prevent redundant API calls
3. Return the same token/response to all callers

## Token Expiration Buffer

The library adds a 30-second buffer before considering a token expired:

```csharp
public bool IsAccessTokenValid => 
    DateTime.UtcNow < ExpiresAt.AddSeconds(-30);
```

This ensures tokens are refreshed before they actually expire.

## Error Handling Strategy

```
HTTP Error (4xx/5xx)
    ↓
BanglalinkAuthenticationException
    ↓
Caught by caller
    ↓
Caller decides action (retry, fail-fast, fallback)
```

Configuration errors are caught early:

```csharp
var client = new BanglalinkAuthClient(httpClient, config);
// Throws BanglalinkConfigurationException if config invalid
```

## Extensibility

### Custom HttpClient Configuration

Via named clients:

```csharp
services.AddHttpClient("Banglalink")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(60))
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, ...));
```

### Custom Token Storage

Replace memory cache with Redis/distributed cache:

```csharp
public class DistributedCacheAuthClient : IBanglalinkAuthClient
{
    private readonly IDistributedCache _cache;
    // ... implement interface
}
```

## Backward Compatibility

Future versions will maintain:
- Same interface contracts
- Same public method signatures
- Same exception types
- Same configuration properties
- Support for .NET 6.0 and later versions

## Performance Considerations

### Token Caching Impact
- Eliminates unnecessary API calls
- Reduces latency (in-memory lookup)
- Reduces load on Banglalink server

### Concurrent Access Impact
- Lock contention minimal (short lock duration)
- Reuses cached tokens across threads
- Prevents thundering herd on token refresh

### Typical Request Flow Duration
- Cached token: ~1-5ms
- Refresh token: ~100-500ms (depends on network)
- Fresh authentication: ~200-800ms (depends on network)

## Security Considerations

1. **Credentials Storage**
   - Never log credentials
   - Use environment variables or secure config
   - Clear credentials from memory when done

2. **Token Transport**
   - Always use HTTPS in production
   - Include Bearer token in Authorization header
   - Never pass tokens in URL or body

3. **Token Validation**
   - Library validates token expiration
   - Server validation performed by Banglalink
   - Implement additional signature validation if needed

4. **Credential Encoding**
   - Basic Auth uses Base64 (encoding, not encryption)
   - Always use over HTTPS
   - Server performs verification

## Testing Patterns

### Unit Testing

```csharp
[Fact]
public void GetValidTokenAsync_WithValidConfig_ReturnToken()
{
    // Arrange
    var mockHttp = new MockHttpMessageHandler();
    var httpClient = new HttpClient(mockHttp);
    var config = GetValidConfiguration();
    var client = new BanglalinkAuthClient(httpClient, config);

    // Act
    var token = await client.GetValidAccessTokenAsync();

    // Assert
    Assert.NotEmpty(token);
}
```

### Integration Testing

```csharp
[Fact]
public async Task Integration_CanAuthenticateWithRealServer()
{
    var config = GetConfigurationFromEnvironment();
    var client = new BanglalinkAuthClient(new HttpClient(), config);
    
    var token = await client.GetValidAccessTokenAsync();
    
    Assert.NotEmpty(token);
}
```

## Versioning

- **Semantic Versioning** (MAJOR.MINOR.PATCH)
- Current version: 1.0.0
- API stability: Stable
