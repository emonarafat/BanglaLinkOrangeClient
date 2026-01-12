# Othoba.BanglaLinkOrange

![Banglalink Orange](BLOrange.png)

A comprehensive C# library for Banglalink Orange OAuth 2.0 authentication and Loyalty API integration.

## Overview

This library provides production-ready implementations for:

1. **OAuth 2.0 Authentication** - Full authentication flow with token management
2. **Loyalty API Client** - Member profile retrieval and tier status checking

Implements the OpenAPI Authorization Documentation v1.1 and supports both the **Password Grant** and **Refresh Token Grant** flows.

**Supports:** .NET 6.0 and .NET 8.0

## Features

### Authentication
✅ **Password Grant Flow** - Initial authentication with username and password  
✅ **Refresh Token Grant Flow** - Automatic token renewal  
✅ **Automatic Token Management** - Built-in token caching and automatic refresh  
✅ **Thread-Safe** - Handles concurrent token access safely  

### Loyalty API
✅ **Member Profile Retrieval** - Get member loyalty information  
✅ **Tier Status Checking** - Check member tier and expiry information  
✅ **Points Analysis** - Enriched data with calculated fields  
✅ **Batch Processing** - Handle multiple member queries efficiently  

### General
✅ **Async/Await Support** - Modern async patterns throughout  
✅ **Dependency Injection** - Seamless integration with .NET DI container  
✅ **Comprehensive Error Handling** - Custom exception types for better error management  
✅ **Configuration Validation** - Validates configuration before use  

## Installation

```bash
dotnet add package Othoba.BanglaLinkOrange
```

## Quick Start

### 🚀 Getting Started in 5 Minutes

**[→ Start Here](START_HERE.md)** - Complete setup guide with step-by-step instructions

For Loyalty API quick start, see [LOYALTY_QUICK_START.md](docs/LOYALTY_QUICK_START.md)

### ✅ Authentication (OAuth 2.0)

#### 1. Basic Usage with Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddBanglalinkAuthClient(config =>
{
    config.BaseUrl = "http://1.2.3.4:8080"; // IP:Port from Banglalink
    config.ClientId = "your-client-id";
    config.ClientSecret = "your-client-secret";
    config.Username = "your-username";
    config.Password = "your-password";
});
```

### ✅ Loyalty API

#### 1. Setup Services

```csharp
// In Program.cs
var loyaltyConfig = new LoyaltyApiConfig
{
    BaseUrl = "https://openapi.banglalink.net/"
};
builder.Services.AddBanglalinkLoyaltyClient(loyaltyConfig);
```

#### 2. Use in Controller or Service

```csharp
[HttpGet("member-profile")]
public async Task<IActionResult> GetMemberProfile([FromQuery] string msisdn)
{
    var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
    return Ok(profile);
}
```

#### 3. Check Member Status

```csharp
var isActive = await _loyaltyService.IsMemberActiveAsync("88014123456789");
var tierStatus = await _loyaltyService.GetMemberTierStatusAsync("88014123456789");
```

---

### ✅ Authentication (OAuth 2.0) - Additional Usage

#### 2. Use in a Service

```csharp
public class MyService
{
    private readonly IBanglalinkAuthClient _authClient;

    public MyService(IBanglalinkAuthClient authClient)
    {
        _authClient = authClient;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // Gets a valid token, automatically refreshing if needed
        return await _authClient.GetValidAccessTokenAsync();
    }
}
```

#### 3. Use with HttpClient for API Calls

```csharp
public class BanglalinkApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IBanglalinkAuthClient _authClient;

    public BanglalinkApiClient(HttpClient httpClient, IBanglalinkAuthClient authClient)
    {
        _httpClient = httpClient;
        _authClient = authClient;
    }

    public async Task<string> CallApiAsync(string endpoint)
    {
        var token = await _authClient.GetValidAccessTokenAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }
}
```

## Documentation

### 📚 Core Documentation

- **[START_HERE.md](START_HERE.md)** - Entry point with 5-minute quick start
- **[LOYALTY_QUICK_START.md](docs/LOYALTY_QUICK_START.md)** - Loyalty API quick setup
- **[LOYALTY_API_GUIDE.md](docs/LOYALTY_API_GUIDE.md)** - Comprehensive Loyalty API guide
- **[LOYALTY_API_EXAMPLES.md](docs/LOYALTY_API_EXAMPLES.md)** - Code examples and patterns
- **[LOYALTY_QUICK_REFERENCE.md](docs/LOYALTY_QUICK_REFERENCE.md)** - Quick lookup reference
- **[DOCUMENTATION_INDEX.md](docs/DOCUMENTATION_INDEX.md)** - Complete documentation index

### 📋 API Specifications

- **[OPENAPI_SPECIFICATION.md](docs/OPENAPI_SPECIFICATION.md)** - OAuth 2.0 implementation details
- **[OPENAPI_Spec_v1.1.pdf](docs/OPENAPI_Spec_v1.1.pdf)** - Official Banglalink OAuth 2.0 specification (PDF)

---

## Configuration

### Mandatory Settings

- **BaseUrl** - The base URL of the Banglalink auth server (e.g., `http://1.2.3.4:8080`)
- **ClientId** - Client ID provided by Banglalink
- **ClientSecret** - Client secret provided by Banglalink
- **Username** - Username provided by Banglalink
- **Password** - Password provided by Banglalink

### Optional Settings

- **Scope** - OAuth scope (default: `"openid"`)
- **HttpClientTimeout** - Timeout for HTTP requests (default: 30 seconds)
- **AutoRefreshToken** - Automatically refresh tokens when they expire (default: `true`)

## API Reference

### IBanglalinkAuthClient

#### Methods

**`GetValidAccessTokenAsync()`**  
Returns a valid access token. Automatically refreshes if the current token is expired.

```csharp
var token = await authClient.GetValidAccessTokenAsync();
```

**`GetValidTokenResponseAsync()`**  
Returns the complete token response with both access and refresh tokens.

```csharp
var tokenResponse = await authClient.GetValidTokenResponseAsync();
Console.WriteLine($"Token: {tokenResponse.AccessToken}");
Console.WriteLine($"Expires in: {tokenResponse.ExpiresIn} seconds");
```

**`AuthenticateAsync()`**  
Performs initial authentication using the configured username and password (Password Grant).

```csharp
var response = await authClient.AuthenticateAsync();
```

**`RefreshTokenAsync(string refreshToken)`**  
Refreshes an access token using a refresh token (Refresh Token Grant).

```csharp
var newTokenResponse = await authClient.RefreshTokenAsync(oldTokenResponse.RefreshToken);
```

**`GetCachedTokenResponse()`**  
Returns the currently cached token response without making any HTTP requests.

```csharp
var cachedToken = authClient.GetCachedTokenResponse();
if (cachedToken != null && cachedToken.IsAccessTokenValid)
{
    Console.WriteLine("Using cached token");
}
```

**`ClearCache()`**  
Clears the cached token response, forcing a new authentication on the next request.

```csharp
authClient.ClearCache();
```

## Token Response Model

The `BanglalinkTokenResponse` class contains:

```csharp
public class BanglalinkTokenResponse
{
    public string AccessToken { get; set; }           // The access token
    public string TokenType { get; set; }              // Token type (e.g., "Bearer")
    public int ExpiresIn { get; set; }                 // Expiration in seconds
    public string RefreshToken { get; set; }           // Token for refreshing
    public int RefreshExpiresIn { get; set; }          // Refresh token expiration
    
    // Computed properties
    public DateTime ExpiresAt { get; }                 // When the token expires
    public bool IsAccessTokenValid { get; }            // Is the access token still valid?
    public bool IsRefreshTokenValid { get; }           // Is the refresh token still valid?
}
```

## Exception Handling

The library defines three exception types:

- **`BanglalinkClientException`** - Base exception for all Banglalink errors
- **`BanglalinkAuthenticationException`** - Thrown when authentication fails
- **`BanglalinkConfigurationException`** - Thrown when configuration is invalid

```csharp
try
{
    var token = await authClient.GetValidAccessTokenAsync();
}
catch (BanglalinkAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (BanglalinkConfigurationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (BanglalinkClientException ex)
{
    Console.WriteLine($"Client error: {ex.Message}");
}
```

## Advanced Scenarios

### Manual Token Management

```csharp
// Authenticate once
var response = await authClient.AuthenticateAsync();
Console.WriteLine($"Access Token: {response.AccessToken}");
Console.WriteLine($"Refresh Token: {response.RefreshToken}");

// Later, refresh the token manually
var newResponse = await authClient.RefreshTokenAsync(response.RefreshToken);
Console.WriteLine($"New Access Token: {newResponse.AccessToken}");
```

### Checking Token Validity

```csharp
var cachedToken = authClient.GetCachedTokenResponse();

if (cachedToken == null)
{
    Console.WriteLine("No token cached. Need to authenticate.");
}
else if (!cachedToken.IsAccessTokenValid)
{
    Console.WriteLine($"Access token expired at {cachedToken.ExpiresAt}");
}
else if (!cachedToken.IsRefreshTokenValid)
{
    Console.WriteLine($"Refresh token expired at {cachedToken.RefreshExpiresAt}");
}
else
{
    Console.WriteLine("Token is valid!");
}
```

### Custom Configuration with Appsettings

```json
{
  "BanglalinkOAuth": {
    "BaseUrl": "http://1.2.3.4:8080",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Username": "your-username",
    "Password": "your-password",
    "Scope": "openid",
    "AutoRefreshToken": true
  }
}
```

```csharp
// In Program.cs
var config = new BanglalinkClientConfiguration();
configuration.GetSection("BanglalinkOAuth").Bind(config);

services.AddBanglalinkAuthClient(config);
```

## API Specification Reference

This library implements the Banglalink OAuth 2.0 Authentication and Authorization API Specification (v1.1).

### Token Endpoint
```
POST http://<IP>:<Port>/auth/realms/banglalink/protocol/openid-connect/token
```

### Request Header
```
Content-Type: application/x-www-form-urlencoded
Authorization: Basic <base64(ClientID:ClientSecret)>
```

### Password Grant Request Body
```
grant_type: password
username: <username>
password: <password>
client_id: <client_id>
scope: openid
```

### Refresh Token Request Body
```
grant_type: refresh_token
refresh_token: <refresh_token>
client_id: <client_id>
```

## Useful Links

- [OAuth 2.0 Overview](https://www.oauth.com/oauth2-servers/access-tokens/)
- [Basic Authentication Generator](https://mixedanalytics.com/tools/basic-authentication-generator/)
- [JWT Token Decoder](https://jwt.io/)

## OpenAPI Specification

This library implements the **Banglalink Orange OpenAPI Authorization Documentation v1.1** and the **Banglalink Loyalty API**.

- 📄 [OPENAPI_SPECIFICATION.md](docs/OPENAPI_SPECIFICATION.md) - OAuth 2.0 implementation details and endpoints
- 📋 [OPENAPI_Spec_v1.1.pdf](docs/OPENAPI_Spec_v1.1.pdf) - Official Banglalink OAuth 2.0 specification (PDF)
- 📄 [LOYALTY_API_GUIDE.md](docs/LOYALTY_API_GUIDE.md) - Complete Loyalty API specification and guide

The specifications are fully incorporated into this project's documentation folder for easy reference.

## License

This library is provided as-is for Banglalink integration purposes.

## Support

- **Banglalink API Hub:** https://apihub.banglalink.net/
- **Email:** api-support@banglalink.net
- **Documentation:** See docs/ folder
- **Issues:** Create an issue on the repository

## Project Status

✅ **OAuth 2.0 Authentication** - Complete and production-ready  
✅ **Loyalty API Client** - Complete and production-ready  
✅ **Comprehensive Documentation** - Complete with 10+ guides and examples  
✅ **Unit Tests** - 15+ test cases with full coverage  

**Current Version:** 1.0.0 | **Status:** Production Ready
