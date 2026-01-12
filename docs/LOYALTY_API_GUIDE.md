# Banglalink Loyalty API Client - Implementation Guide

## Overview

This guide provides comprehensive documentation for implementing and using the Banglalink Loyalty API client. The Loyalty API allows you to retrieve member loyalty profile information including tier status, available points, and enrollment details.

## API Specification

### Endpoint

```
POST /openapi-lms/loyalty2/get-member-profile
```

### Authentication

- **Method:** OAuth 2.0 Bearer Token
- **Header:** `Authorization: Bearer <access_token>`
- **Content-Type:** `application/vnd.banglalink.apihub-v1.0+json`

### Request Body

```json
{
  "channel": "LMSMYBLAPP",
  "msisdn": "88014123456789",
  "transactionID": "LMS34197492"
}
```

**Parameters:**
- `channel` (string): Channel name identifying the source of the request
- `msisdn` (string): Customer Mobile Station Integrated Services Digital Network (88014XXXXXXXXX)
- `transactionID` (string): Unique transaction identifier

### Response Body

```json
{
  "msisdn": "88014123456789",
  "transactionID": "LMS34197492",
  "statusCode": "0",
  "statusMsg": "SUCCESS",
  "responseDateTime": "10-07-2023 14:49:19",
  "loyaltyProfileInfo": {
    "availablePoints": "10409080",
    "currentTierLevel": "SIGNATURE",
    "expiryDate": "31-12-2024",
    "pointsExpiring": "10409080",
    "enrolledDate": "21-11-2022 10:31:30",
    "enrolledChannel": "MYBL"
  }
}
```

**Response Fields:**
- `msisdn`: Customer mobile number
- `transactionID`: Echo of request transaction ID
- `statusCode`: API response code ("0" = success)
- `statusMsg`: Human-readable status message
- `responseDateTime`: Response timestamp
- `loyaltyProfileInfo`: Member loyalty information
  - `availablePoints`: Total available loyalty points
  - `currentTierLevel`: Current tier (e.g., "SIGNATURE", "GOLD", "PLATINUM")
  - `expiryDate`: Tier expiry date (DD-MM-YYYY format)
  - `pointsExpiring`: Points set to expire soon
  - `enrolledDate`: Member enrollment date and time
  - `enrolledChannel`: Channel through which member enrolled

## Implementation

### 1. Installation

The Loyalty API client is included in the `Othoba.BanglaLinkOrange` NuGet package:

```bash
dotnet add package Othoba.BanglaLinkOrange
```

### 2. Configuration

Add configuration to `appsettings.json`:

```json
{
  "BanglalinkOAuth": {
    "BaseUrl": "https://api.banglalink.net/oauth2/",
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "Username": "your_username",
    "Password": "your_password"
  },
  "LoyaltyApi": {
    "BaseUrl": "https://openapi.banglalink.net/",
    "Timeout": "00:00:30",
    "RetryCount": 3,
    "RetryDelay": "00:00:00.5000000"
  }
}
```

### 3. Service Registration (Startup Configuration)

In your `Program.cs` file:

```csharp
using Othoba.BanglaLinkOrange;
using WebApiExample.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register Authentication Client
builder.Services.AddBanglalinkAuthClient(config =>
{
    var banglalinkSection = builder.Configuration.GetSection("BanglalinkOAuth");
    config.BaseUrl = banglalinkSection["BaseUrl"] ?? throw new InvalidOperationException();
    config.ClientId = banglalinkSection["ClientId"] ?? throw new InvalidOperationException();
    config.ClientSecret = banglalinkSection["ClientSecret"] ?? throw new InvalidOperationException();
    config.Username = banglalinkSection["Username"] ?? throw new InvalidOperationException();
    config.Password = banglalinkSection["Password"] ?? throw new InvalidOperationException();
});

// Register Loyalty Client and Service
var loyaltyConfig = new LoyaltyApiConfig();
builder.Configuration.GetSection("LoyaltyApi").Bind(loyaltyConfig);
builder.Services.AddBanglalinkLoyaltyClient(loyaltyConfig);

var app = builder.Build();
app.MapControllers();
await app.RunAsync();
```

### 4. Usage Examples

#### Using ILoyaltyClient Directly

```csharp
public class MyService
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;

    public MyService(ILoyaltyClient loyaltyClient, IBanglalinkAuthClient authClient)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
    }

    public async Task<LoyaltyMemberProfileResponse> GetMemberProfile(string msisdn)
    {
        // Get access token
        var accessToken = await _authClient.GetValidAccessTokenAsync();

        // Create request
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = "LMSMYBLAPP",
            Msisdn = msisdn,
            TransactionID = Guid.NewGuid().ToString()
        };

        // Call API
        var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken);

        return response;
    }
}
```

#### Using ILoyaltyService (Recommended)

The `ILoyaltyService` provides a higher-level business logic layer:

```csharp
[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpGet("member-profile")]
    public async Task<IActionResult> GetMemberProfile([FromQuery] string msisdn)
    {
        try
        {
            var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("member-tier-status")]
    public async Task<IActionResult> GetTierStatus([FromQuery] string msisdn)
    {
        var tierStatus = await _loyaltyService.GetMemberTierStatusAsync(msisdn);
        return Ok(tierStatus);
    }

    [HttpGet("is-member-active")]
    public async Task<IActionResult> IsMemberActive([FromQuery] string msisdn)
    {
        var isActive = await _loyaltyService.IsMemberActiveAsync(msisdn);
        return Ok(new { msisdn, isActive });
    }
}
```

### 5. Error Handling

The implementation includes comprehensive error handling:

```csharp
try
{
    var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
}
catch (ArgumentException ex)
{
    // Invalid input (e.g., empty MSISDN)
    _logger.LogWarning("Invalid request: {Message}", ex.Message);
}
catch (LoyaltyApiException ex)
{
    // API returned an error
    _logger.LogError("API error: {Message}, Code: {ErrorCode}", ex.Message, ex.ErrorCode);
}
catch (BanglalinkAuthenticationException ex)
{
    // Authentication failed
    _logger.LogError("Authentication failed: {Message}", ex.Message);
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError("Unexpected error: {Message}", ex.Message);
}
```

## Key Classes

### LoyaltyMemberProfileRequest

Represents the request to get member profile:

```csharp
public class LoyaltyMemberProfileRequest
{
    public string Channel { get; set; }
    public string Msisdn { get; set; }
    public string TransactionID { get; set; }
}
```

### LoyaltyMemberProfileResponse

Represents the API response:

```csharp
public class LoyaltyMemberProfileResponse
{
    public string Msisdn { get; set; }
    public string TransactionID { get; set; }
    public string StatusCode { get; set; }
    public string StatusMsg { get; set; }
    public string ResponseDateTime { get; set; }
    public LoyaltyProfileInfo? LoyaltyProfileInfo { get; set; }
}
```

### LoyaltyProfileInfo

Contains member loyalty details:

```csharp
public class LoyaltyProfileInfo
{
    public string AvailablePoints { get; set; }
    public string CurrentTierLevel { get; set; }
    public string ExpiryDate { get; set; }
    public string PointsExpiring { get; set; }
    public string EnrolledDate { get; set; }
    public string EnrolledChannel { get; set; }
}
```

### ILoyaltyService

High-level service interface:

```csharp
public interface ILoyaltyService
{
    Task<MemberLoyaltyProfile> GetMemberProfileAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default);

    Task<EnrichedMemberLoyaltyProfile> GetEnrichedMemberProfileAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default);

    Task<bool> IsMemberActiveAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default);

    Task<MemberTierStatus> GetMemberTierStatusAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default);
}
```

## Testing

### Unit Tests

The project includes comprehensive unit tests using Moq and xUnit:

```csharp
[Fact]
public async Task GetMemberProfile_WithValidMsisdn_ReturnsProfile()
{
    // Arrange
    var msisdn = "88014123456789";
    var accessToken = "valid_token";

    _mockAuthClient
        .Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(accessToken);

    _mockLoyaltyClient
        .Setup(x => x.GetMemberProfileAsync(It.IsAny<LoyaltyMemberProfileRequest>(), accessToken, It.IsAny<CancellationToken>()))
        .ReturnsAsync(/* mock response */);

    // Act
    var result = await _loyaltyService.GetMemberProfileAsync(msisdn);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(msisdn, result.Msisdn);
}
```

Run tests:

```bash
dotnet test
```

### Integration Tests

Integration tests can be run against a real Banglalink environment with valid credentials:

```bash
# Set environment variables
$env:BANGLALINK_CLIENT_ID = "your_client_id"
$env:BANGLALINK_CLIENT_SECRET = "your_client_secret"

# Run tests (skipped by default)
dotnet test --logger "console;verbosity=detailed"
```

## Best Practices

### 1. Token Management

Always use `IBanglalinkAuthClient.GetValidAccessTokenAsync()` which handles token caching:

```csharp
// ✓ Good - automatic token caching and refresh
var token = await _authClient.GetValidAccessTokenAsync();

// ✗ Bad - creates new token each time
var token = await _authClient.RequestAccessTokenAsync();
```

### 2. Error Handling

Always implement proper error handling:

```csharp
// ✓ Good
try
{
    var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
}
catch (ArgumentException) { /* Handle validation */ }
catch (LoyaltyApiException) { /* Handle API errors */ }
catch (Exception) { /* Handle unexpected errors */ }

// ✗ Bad
var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
```

### 3. Validation

Always validate input before calling APIs:

```csharp
// ✓ Good
if (string.IsNullOrWhiteSpace(msisdn))
    throw new ArgumentException("MSISDN is required", nameof(msisdn));

// ✗ Bad
var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
```

### 4. Logging

Use structured logging:

```csharp
// ✓ Good
_logger.LogInformation("Retrieving profile for MSISDN: {Msisdn}", msisdn);

// ✗ Bad
_logger.LogInformation($"Retrieving profile for MSISDN: {msisdn}");
```

### 5. Cancellation Tokens

Always support cancellation:

```csharp
// ✓ Good
public async Task MyMethodAsync(CancellationToken cancellationToken = default)
{
    var profile = await _loyaltyService.GetMemberProfileAsync(
        msisdn,
        cancellationToken: cancellationToken);
}

// ✗ Bad
var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
```

## Performance Considerations

### Caching

Consider implementing caching for member profiles:

```csharp
public class CachedLoyaltyService : ILoyaltyService
{
    private readonly IMemoryCache _cache;
    private readonly ILoyaltyService _innerService;

    public async Task<MemberLoyaltyProfile> GetMemberProfileAsync(string msisdn, ...)
    {
        var cacheKey = $"loyalty:profile:{msisdn}";

        if (_cache.TryGetValue(cacheKey, out MemberLoyaltyProfile cached))
            return cached;

        var profile = await _innerService.GetMemberProfileAsync(msisdn, ...);

        _cache.Set(cacheKey, profile, TimeSpan.FromMinutes(5));

        return profile;
    }
}
```

### Timeouts

Configure appropriate timeouts:

```json
{
  "LoyaltyApi": {
    "Timeout": "00:00:30",      // 30 seconds
    "RetryCount": 3,
    "RetryDelay": "00:00:00.5"  // Start with 500ms
  }
}
```

## Troubleshooting

### Common Issues

#### 1. Authentication Error

**Problem:** `401 Unauthorized`

**Solution:**
- Verify OAuth credentials in `appsettings.json`
- Check token expiration with `GetValidAccessTokenAsync()`
- Ensure client credentials are not revoked

#### 2. Member Not Found

**Problem:** `statusCode: "1"` with member not found message

**Solution:**
- Verify MSISDN format: `88014XXXXXXXXX`
- Check if member is enrolled in loyalty program
- Confirm MSISDN exists in Banglalink system

#### 3. Timeout

**Problem:** Request takes too long or times out

**Solution:**
- Increase timeout in configuration
- Check network connectivity
- Verify API endpoint is accessible

#### 4. Invalid Channel

**Problem:** API returns error for invalid channel

**Solution:**
- Use correct channel name (e.g., "LMSMYBLAPP")
- Verify channel is registered with Banglalink
- Check if channel is enabled for Loyalty API

## Examples

### Complete Web API Example

See the `WebApiExample-Net8` project for a complete working example with:
- Dependency injection configuration
- Error handling
- Logging
- Swagger documentation
- Health checks
- CORS configuration

### Console Application Example

```csharp
// Create clients
var authClient = new BanglalinkAuthClient2(
    "https://api.banglalink.net/oauth2/",
    "client_id",
    "client_secret",
    logger);

var loyaltyClient = new LoyaltyClient(
    "https://openapi.banglalink.net/",
    logger);

// Get token
var token = await authClient.GetValidAccessTokenAsync();

// Create request
var request = new LoyaltyMemberProfileRequest
{
    Channel = "LMSMYBLAPP",
    Msisdn = "88014123456789",
    TransactionID = Guid.NewGuid().ToString()
};

// Call API
var response = await loyaltyClient.GetMemberProfileAsync(request, token);

// Use response
Console.WriteLine($"Member: {response.Msisdn}");
Console.WriteLine($"Points: {response.LoyaltyProfileInfo?.AvailablePoints}");
Console.WriteLine($"Tier: {response.LoyaltyProfileInfo?.CurrentTierLevel}");
```

## Support

For support and issues:
- **Email:** api-support@banglalink.net
- **Documentation:** [Banglalink API Hub](https://apihub.banglalink.net/)
- **GitHub Issues:** Report issues in the project repository

## License

This implementation is part of the Banglalink Orange Client project. See LICENSE file for details.

## Version History

### v1.0.0 (Current)
- Initial implementation of Loyalty API client
- Support for get-member-profile endpoint
- Full error handling and logging
- Comprehensive documentation and examples
- Unit and integration tests

## FAQ

**Q: How often should I cache member profiles?**
A: Recommend 5-10 minutes depending on use case. Points and tier information may change.

**Q: Can I reuse the same access token?**
A: Yes! Use `GetValidAccessTokenAsync()` which automatically manages token lifecycle.

**Q: What's the MSISDN format?**
A: Bangladesh numbers: `88014XXXXXXXXX` (11 digits starting with 880)

**Q: How do I handle rate limiting?**
A: Implement exponential backoff using the built-in retry configuration.

**Q: Is PII data encrypted?**
A: All communication uses HTTPS with TLS 1.2+
