# API Reference

## Table of Contents

1. [IBanglalinkAuthClient Interface](#ibanglalinkAuthclient-interface)
2. [BanglalinkAuthClient Implementation](#banglalinkAuthclient-implementation)
3. [Models](#models)
4. [Configuration](#configuration)
5. [Exceptions](#exceptions)
6. [Utilities](#utilities)
7. [Extension Methods](#extension-methods)

---

## IBanglalinkAuthClient Interface

The primary interface for all authentication operations.

### Methods

#### `GetValidAccessTokenAsync()`

Returns a valid access token string. Automatically refreshes if the cached token is expired.

```csharp
Task<string> GetValidAccessTokenAsync()
```

**Returns:** `Task<string>` - The access token

**Exceptions:**
- `BanglalinkAuthenticationException` - If authentication fails
- `BanglalinkConfigurationException` - If configuration is invalid

**Example:**
```csharp
var token = await authClient.GetValidAccessTokenAsync();
var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
```

---

#### `GetValidTokenResponseAsync()`

Returns the complete token response with all token data.

```csharp
Task<BanglalinkTokenResponse> GetValidTokenResponseAsync()
```

**Returns:** `Task<BanglalinkTokenResponse>` - The complete token response

**Exceptions:**
- `BanglalinkAuthenticationException` - If authentication fails
- `BanglalinkConfigurationException` - If configuration is invalid

**Example:**
```csharp
var response = await authClient.GetValidTokenResponseAsync();
Console.WriteLine($"Expires in: {response.ExpiresIn} seconds");
Console.WriteLine($"Token type: {response.TokenType}");
```

---

#### `AuthenticateAsync()`

Performs initial authentication using the Password Grant flow (username and password).

```csharp
Task<BanglalinkTokenResponse> AuthenticateAsync()
```

**Returns:** `Task<BanglalinkTokenResponse>` - The token response

**Exceptions:**
- `BanglalinkAuthenticationException` - If credentials are invalid

**Remarks:**
- Clears cached token and performs fresh authentication
- Uses the username and password from configuration
- Token is cached after successful authentication

**Example:**
```csharp
var response = await authClient.AuthenticateAsync();
```

---

#### `RefreshTokenAsync(string refreshToken)`

Refreshes the access token using a refresh token (Refresh Token Grant flow).

```csharp
Task<BanglalinkTokenResponse> RefreshTokenAsync(string refreshToken)
```

**Parameters:**
- `refreshToken` - The refresh token from a previous authentication

**Returns:** `Task<BanglalinkTokenResponse>` - The new token response

**Exceptions:**
- `ArgumentException` - If refreshToken is null or empty
- `BanglalinkAuthenticationException` - If refresh fails

**Example:**
```csharp
var newResponse = await authClient.RefreshTokenAsync(oldResponse.RefreshToken);
```

---

#### `GetCachedTokenResponse()`

Gets the currently cached token response without making any HTTP requests.

```csharp
BanglalinkTokenResponse? GetCachedTokenResponse()
```

**Returns:** `BanglalinkTokenResponse?` - The cached token response, or null if nothing cached

**Example:**
```csharp
var cached = authClient.GetCachedTokenResponse();
if (cached != null && cached.IsAccessTokenValid)
{
    Console.WriteLine("Using cached token");
}
```

---

#### `ClearCache()`

Clears the cached token response.

```csharp
void ClearCache()
```

**Remarks:**
- The next call to `GetValidAccessTokenAsync()` will authenticate fresh
- Useful for forcing re-authentication

**Example:**
```csharp
authClient.ClearCache();
var newToken = await authClient.GetValidAccessTokenAsync();
```

---

## BanglalinkAuthClient Implementation

Direct implementation of `IBanglalinkAuthClient`.

### Constructor

```csharp
public BanglalinkAuthClient(
    HttpClient httpClient, 
    BanglalinkClientConfiguration configuration)
```

**Parameters:**
- `httpClient` - The HTTP client to use for requests
- `configuration` - The Banglalink client configuration

**Exceptions:**
- `ArgumentNullException` - If httpClient or configuration is null
- `BanglalinkConfigurationException` - If configuration is invalid

**Example:**
```csharp
var config = new BanglalinkClientConfiguration
{
    BaseUrl = "http://1.2.3.4:8080",
    ClientId = "id",
    ClientSecret = "secret",
    Username = "user",
    Password = "pass"
};

var httpClient = new HttpClient();
var authClient = new BanglalinkAuthClient(httpClient, config);
```

---

## Models

### BanglalinkTokenResponse

Represents the OAuth 2.0 token response from Banglalink.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `AccessToken` | `string` | The access token for API calls |
| `TokenType` | `string` | Token type (default: "Bearer") |
| `ExpiresIn` | `int` | Token expiration in seconds |
| `RefreshToken` | `string` | Token for refreshing the access token |
| `RefreshExpiresIn` | `int` | Refresh token expiration in seconds |
| `ExpiresAt` | `DateTime` | Calculated UTC expiration time |
| `RefreshExpiresAt` | `DateTime` | Calculated refresh token expiration time |
| `IsAccessTokenValid` | `bool` | Is the access token still valid? (includes 30-sec buffer) |
| `IsRefreshTokenValid` | `bool` | Is the refresh token still valid? (includes 30-sec buffer) |

**Example:**
```csharp
var response = await authClient.GetValidTokenResponseAsync();

Console.WriteLine($"Access Token: {response.AccessToken}");
Console.WriteLine($"Token Type: {response.TokenType}");
Console.WriteLine($"Expires In: {response.ExpiresIn} seconds");
Console.WriteLine($"Expires At: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"Valid: {response.IsAccessTokenValid}");

if (response.IsAccessTokenValid)
{
    var remaining = response.ExpiresAt - DateTime.UtcNow;
    Console.WriteLine($"Time Remaining: {remaining.TotalSeconds:F0} seconds");
}
```

---

## Configuration

### BanglalinkClientConfiguration

Configuration settings for the Banglalink OAuth 2.0 client.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `string` | `""` | Base URL of the authorization server |
| `ClientId` | `string` | `""` | Client ID provided by Banglalink |
| `ClientSecret` | `string` | `""` | Client secret provided by Banglalink |
| `Username` | `string` | `""` | Username for authentication |
| `Password` | `string` | `""` | Password for authentication |
| `Scope` | `string` | `"openid"` | OAuth scope |
| `HttpClientTimeout` | `TimeSpan` | 30 seconds | Timeout for HTTP requests |
| `AutoRefreshToken` | `bool` | `true` | Auto-refresh tokens when expired |

#### Methods

**`IsValid()`**

Validates that all required configuration is present.

```csharp
bool IsValid()
```

**Returns:** `true` if configuration is valid; otherwise `false`

**Example:**
```csharp
var config = new BanglalinkClientConfiguration { /* ... */ };
if (!config.IsValid())
{
    throw new Exception("Invalid configuration");
}
```

---

**`GetValidationErrors()`**

Gets a list of validation error messages.

```csharp
List<string> GetValidationErrors()
```

**Returns:** List of error messages for missing/invalid properties

**Example:**
```csharp
var config = new BanglalinkClientConfiguration { /* ... */ };
if (!config.IsValid())
{
    var errors = config.GetValidationErrors();
    foreach (var error in errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

---

## Exceptions

### BanglalinkClientException

Base exception for all Banglalink client operations.

```csharp
public class BanglalinkClientException : Exception
{
    public BanglalinkClientException(string message) { }
    public BanglalinkClientException(string message, Exception innerException) { }
}
```

---

### BanglalinkAuthenticationException

Thrown when authentication operations fail.

```csharp
public class BanglalinkAuthenticationException : BanglalinkClientException
{
    public BanglalinkAuthenticationException(string message) { }
    public BanglalinkAuthenticationException(string message, Exception innerException) { }
}
```

**Scenarios:**
- Invalid credentials
- HTTP request failure
- Server returns error status
- Token parsing fails

---

### BanglalinkConfigurationException

Thrown when configuration is invalid.

```csharp
public class BanglalinkConfigurationException : BanglalinkClientException
{
    public BanglalinkConfigurationException(string message) { }
    public BanglalinkConfigurationException(string message, Exception innerException) { }
}
```

**Scenarios:**
- Missing required configuration
- Invalid configuration values

**Example:**
```csharp
try
{
    var client = new BanglalinkAuthClient(httpClient, config);
}
catch (BanglalinkConfigurationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

---

## Utilities

### BasicAuthenticationGenerator

Utility for generating Basic Authentication tokens.

#### Methods

**`GenerateToken(string clientId, string clientSecret)`**

Generates a Base64-encoded Basic Auth token.

```csharp
static string GenerateToken(string clientId, string clientSecret)
```

**Parameters:**
- `clientId` - The client ID
- `clientSecret` - The client secret

**Returns:** Base64-encoded `client_id:client_secret`

**Exceptions:**
- `ArgumentNullException` - If clientId or clientSecret is null

**Remarks:**
- Used internally by the client
- Format: `base64(client_id:client_secret)`

**Example:**
```csharp
var basicAuth = BasicAuthenticationGenerator.GenerateToken("myid", "mysecret");
// Output example: "bXlpZDpteXNlY3JldA=="

var request = new HttpRequestMessage(HttpMethod.Post, url);
request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
```

---

## Extension Methods

### ServiceCollectionExtensions

Extensions for registering the client in the DI container.

#### `AddBanglalinkAuthClient(this IServiceCollection, Action<BanglalinkClientConfiguration>)`

Registers the client with configuration action.

```csharp
public static IServiceCollection AddBanglalinkAuthClient(
    this IServiceCollection services,
    Action<BanglalinkClientConfiguration> configure)
```

**Parameters:**
- `services` - The service collection
- `configure` - Action to configure the settings

**Returns:** The service collection (for chaining)

**Example:**
```csharp
services.AddBanglalinkAuthClient(config =>
{
    config.BaseUrl = "http://1.2.3.4:8080";
    config.ClientId = "id";
    config.ClientSecret = "secret";
    config.Username = "user";
    config.Password = "pass";
});
```

---

#### `AddBanglalinkAuthClient(this IServiceCollection, BanglalinkClientConfiguration)`

Registers the client with a configuration instance.

```csharp
public static IServiceCollection AddBanglalinkAuthClient(
    this IServiceCollection services,
    BanglalinkClientConfiguration configuration)
```

**Parameters:**
- `services` - The service collection
- `configuration` - The configuration instance

**Returns:** The service collection (for chaining)

**Example:**
```csharp
var config = new BanglalinkClientConfiguration { /* ... */ };
services.AddBanglalinkAuthClient(config);

var client = serviceProvider.GetRequiredService<IBanglalinkAuthClient>();
```

---

## Common Patterns

### Basic Usage Pattern

```csharp
var token = await authClient.GetValidAccessTokenAsync();
var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
var response = await httpClient.SendAsync(request);
```

### Error Handling Pattern

```csharp
try
{
    var token = await authClient.GetValidAccessTokenAsync();
}
catch (BanglalinkAuthenticationException ex)
{
    logger.LogError("Authentication failed: {0}", ex.Message);
    // Handle authentication failure
}
catch (BanglalinkConfigurationException ex)
{
    logger.LogError("Configuration error: {0}", ex.Message);
    // Handle configuration error
}
catch (BanglalinkClientException ex)
{
    logger.LogError("Client error: {0}", ex.Message);
    // Handle other errors
}
```

### Token Validation Pattern

```csharp
var cachedToken = authClient.GetCachedTokenResponse();

if (cachedToken == null)
{
    // No token available
    var token = await authClient.GetValidAccessTokenAsync();
}
else if (cachedToken.IsAccessTokenValid)
{
    // Use cached token
    return cachedToken.AccessToken;
}
else if (cachedToken.IsRefreshTokenValid)
{
    // Refresh token
    var newResponse = await authClient.RefreshTokenAsync(cachedToken.RefreshToken);
    return newResponse.AccessToken;
}
else
{
    // Both expired, authenticate again
    var newResponse = await authClient.AuthenticateAsync();
    return newResponse.AccessToken;
}
```

---

## Thread Safety

All methods are thread-safe for concurrent access. The library uses internal locking to ensure:
- Consistent token state across threads
- No redundant API calls during refresh
- Safe cache access from multiple threads

---

## Performance Tips

1. **Reuse HttpClient:** Create a single `HttpClient` instance and reuse it
2. **Use DI:** Register client in DI container for lifecycle management
3. **Monitor Tokens:** Check `GetCachedTokenResponse()` to track token status
4. **Handle Errors:** Implement proper error handling for network failures

---

## Version Compatibility

- Minimum .NET version: 6.0
- Also supports .NET 8.0+
- Uses modern C# language features (nullable reference types, records where applicable)
- Compatible with dependency injection frameworks that implement `IServiceCollection`
