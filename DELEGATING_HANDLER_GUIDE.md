# Authentication Delegating Handler

## Overview

The `AuthenticationDelegatingHandler` is a `DelegatingHandler` that automatically injects Bearer tokens into HTTP requests made by the `LoyaltyClient`. This eliminates the need to manually pass access tokens on each API call.

## How It Works

1. **Automatic Token Injection**: The handler intercepts all outgoing HTTP requests
2. **Token Retrieval**: Gets the access token from the registered `IBanglalinkAuthClient`
3. **Bearer Token Addition**: Adds the `Authorization: Bearer {token}` header to requests
4. **Conditional Application**: Only adds the header if:
   - An `IBanglalinkAuthClient` is available (configured in DI)
   - No `Authorization` header already exists in the request

## Architecture

```
Client Request
    ↓
AuthenticationDelegatingHandler (Automatic Token Injection)
    ↓
HttpClient (with configured timeout and retry policies)
    ↓
Banglalink Loyalty API
```

## Setup

### 1. Register in Dependency Injection

The handler is automatically registered when using the unified configuration:

```csharp
// In Program.cs
builder.Services.AddBanglalink(builder.Configuration);
```

This registration includes:
- `IBanglalinkAuthClient` (OAuth client with token management)
- `AuthenticationDelegatingHandler` (automatic token injection)
- `ILoyaltyClient` (loyalty API client)

### 2. Configure appsettings.json

```json
{
  "Banglalink": {
    "OAuth": {
      "BaseUrl": "https://api.banglalink.net/oauth2/",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Username": "your-username",
      "Password": "your-password"
    },
    "Loyalty": {
      "BaseUrl": "https://openapi.banglalink.net/",
      "Timeout": 30,
      "RetryCount": 3,
      "RetryDelay": 500
    }
  }
}
```

## Usage

### Simple Usage (Recommended)

No need to manually manage tokens - just pass the request:

```csharp
[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyClient _loyaltyClient;

    public LoyaltyController(ILoyaltyClient loyaltyClient)
    {
        _loyaltyClient = loyaltyClient;
    }

    [HttpGet("member-profile")]
    public async Task<IActionResult> GetMemberProfile(string msisdn)
    {
        var request = new LoyaltyMemberProfileRequest
        {
            Msisdn = msisdn,
            Channel = "Web",
            TransactionID = Guid.NewGuid().ToString()
        };

        // Token is automatically injected by AuthenticationDelegatingHandler
        var response = await _loyaltyClient.GetMemberProfileAsync(request);
        
        return Ok(response);
    }
}
```

### With Explicit Token (Legacy Support)

For backward compatibility, you can still pass an explicit token:

```csharp
var request = new LoyaltyMemberProfileRequest { /* ... */ };
var explicitToken = "your-access-token";

// This method is marked as [Obsolete] but still works
var response = await _loyaltyClient.GetMemberProfileAsync(request, explicitToken);
```

## Token Lifecycle Management

The handler automatically manages the token lifecycle:

1. **Initial Authentication**: `IBanglalinkAuthClient` authenticates with OAuth
2. **Token Caching**: Tokens are cached to avoid unnecessary requests
3. **Automatic Refresh**: Expired tokens are automatically refreshed
4. **Concurrent Access**: Thread-safe token management for concurrent requests

```csharp
// Example: Multiple concurrent requests with automatic token management
var tasks = new List<Task<LoyaltyMemberProfileResponse>>();

foreach (var msisdn in msisdnList)
{
    var request = new LoyaltyMemberProfileRequest 
    { 
        Msisdn = msisdn, 
        Channel = "Web" 
    };
    
    // All requests share the same managed token
    tasks.Add(_loyaltyClient.GetMemberProfileAsync(request));
}

var results = await Task.WhenAll(tasks);
```

## Error Handling

The handler integrates with Polly retry policies:

```csharp
// Configuration: Retry up to 3 times with exponential backoff
services.AddHttpClient<ILoyaltyClient, LoyaltyClient>()
    .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
    .AddTransientHttpErrorPolicy(p => 
        p.WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100)
        )
    );
```

Common scenarios:
- **401 Unauthorized**: Token is refreshed automatically
- **Network Timeout**: Retry logic handles transient failures
- **API Errors**: Proper exceptions with context information

## Advanced: Custom Token Provider

If you need custom token management, you can provide your own `IBanglalinkAuthClient`:

```csharp
// Implement custom auth client
public class CustomAuthClient : IBanglalinkAuthClient
{
    public async Task<string> GetValidAccessTokenAsync()
    {
        // Your custom token retrieval logic
        return await FetchTokenFromCustomSource();
    }

    public async Task RefreshAccessTokenAsync()
    {
        // Your custom refresh logic
    }
}

// Register in DI
builder.Services.AddSingleton<IBanglalinkAuthClient, CustomAuthClient>();
```

## Performance Considerations

1. **Token Caching**: Tokens are cached to avoid repeated authentication
2. **Single Request Header Addition**: Minimal overhead per request
3. **Concurrent Requests**: Handler is thread-safe and efficient
4. **Connection Pooling**: HttpClient manages connection pooling automatically

## Security Best Practices

1. **Token Storage**: Never hardcode tokens in code
2. **Configuration**: Use environment variables or secure configuration providers
3. **HTTPS**: Always use HTTPS for API calls
4. **Timeout**: Configure appropriate timeouts to prevent hanging requests
5. **Error Messages**: Don't expose sensitive token information in error logs

## Testing

### Unit Tests

```csharp
[Fact]
public async Task GetMemberProfileAsync_WithDelegatingHandler_InjectsToken()
{
    // Arrange
    var mockAuthClient = new Mock<IBanglalinkAuthClient>();
    mockAuthClient.Setup(x => x.GetValidAccessTokenAsync())
        .ReturnsAsync("test-token");

    var handler = new AuthenticationDelegatingHandler(mockAuthClient.Object);
    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

    // Act
    // Simulate handler processing (in real scenario, via HttpClient pipeline)
    await handler.SendAsync(request, CancellationToken.None);

    // Assert
    Assert.Contains("Authorization", request.Headers.Select(h => h.Key));
    Assert.Equal("Bearer test-token", request.Headers.Authorization?.ToString());
}
```

### Integration Tests

```csharp
[Fact]
public async Task LoyaltyClient_WithDelegatingHandler_AuthenticatesRequests()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddBanglalink(configuration);
    var provider = services.BuildServiceProvider();

    var loyaltyClient = provider.GetRequiredService<ILoyaltyClient>();

    // Act
    var response = await loyaltyClient.GetMemberProfileAsync(
        new LoyaltyMemberProfileRequest 
        { 
            Msisdn = "8801234567890",
            Channel = "Web"
        }
    );

    // Assert
    Assert.NotNull(response);
    Assert.True(response.IsSuccessful);
}
```

## Troubleshooting

### Issue: Authorization Header Not Being Added

**Solution**: Verify that:
1. `IBanglalinkAuthClient` is registered in DI
2. OAuth credentials are configured correctly
3. No explicit `Authorization` header is already set

### Issue: Token Expired Errors

**Solution**: Ensure that:
1. Token refresh mechanism is working
2. System clock is synchronized
3. OAuth client has valid refresh token

### Issue: Concurrent Request Issues

**Solution**: The handler is thread-safe by default. If issues occur:
1. Check for custom `IBanglalinkAuthClient` implementation
2. Verify token cache synchronization
3. Review concurrent request patterns

## Migration Guide

### From Manual Token Management

**Before** (Manual token passing):
```csharp
var token = await authClient.GetValidAccessTokenAsync();
var response = await loyaltyClient.GetMemberProfileAsync(request, token);
```

**After** (Automatic token injection):
```csharp
var response = await loyaltyClient.GetMemberProfileAsync(request);
```

## Related Documentation

- [Quick Start Guide](LOYALTY_QUICK_START.md)
- [API Reference](LOYALTY_API_GUIDE.md)
- [Configuration](../docs/configuration.md)
- [Error Handling](../docs/error-handling.md)

---

**Version**: 1.0.1+
**Last Updated**: January 12, 2026
**Status**: Production Ready
