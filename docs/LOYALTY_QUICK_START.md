# Banglalink Loyalty API - Quick Start Guide

## 5-Minute Setup

### 1. Add NuGet Package

```bash
dotnet add package Othoba.BanglaLinkOrange
```

### 2. Configure appsettings.json

```json
{
  "BanglalinkOAuth": {
    "BaseUrl": "https://api.banglalink.net/oauth2/",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "Username": "YOUR_USERNAME",
    "Password": "YOUR_PASSWORD"
  },
  "LoyaltyApi": {
    "BaseUrl": "https://openapi.banglalink.net/",
    "Timeout": "00:00:30",
    "RetryCount": 3,
    "RetryDelay": "00:00:00.5"
  }
}
```

### 3. Register Services in Program.cs

```csharp
using Othoba.BanglaLinkOrange;
using WebApiExample.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register Auth Client
builder.Services.AddBanglalinkAuthClient(config =>
{
    var section = builder.Configuration.GetSection("BanglalinkOAuth");
    config.BaseUrl = section["BaseUrl"] ?? throw new InvalidOperationException();
    config.ClientId = section["ClientId"] ?? throw new InvalidOperationException();
    config.ClientSecret = section["ClientSecret"] ?? throw new InvalidOperationException();
    config.Username = section["Username"] ?? throw new InvalidOperationException();
    config.Password = section["Password"] ?? throw new InvalidOperationException();
});

// Register Loyalty Client
var loyaltyConfig = new LoyaltyApiConfig();
builder.Configuration.GetSection("LoyaltyApi").Bind(loyaltyConfig);
builder.Services.AddBanglalinkLoyaltyClient(loyaltyConfig);

var app = builder.Build();
app.MapControllers();
await app.RunAsync();
```

### 4. Create Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Othoba.BanglaLinkOrange.Models;
using WebApiExample.Services;

[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;
    private readonly ILogger<LoyaltyController> _logger;

    public LoyaltyController(ILoyaltyService loyaltyService, ILogger<LoyaltyController> logger)
    {
        _loyaltyService = loyaltyService;
        _logger = logger;
    }

    [HttpGet("member-profile")]
    public async Task<IActionResult> GetMemberProfile([FromQuery] string msisdn)
    {
        try
        {
            _logger.LogInformation("Getting profile for {Msisdn}", msisdn);
            var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("member-tier")]
    public async Task<IActionResult> GetMemberTier([FromQuery] string msisdn)
    {
        var tierStatus = await _loyaltyService.GetMemberTierStatusAsync(msisdn);
        return Ok(tierStatus);
    }

    [HttpGet("is-active")]
    public async Task<IActionResult> IsMemberActive([FromQuery] string msisdn)
    {
        var isActive = await _loyaltyService.IsMemberActiveAsync(msisdn);
        return Ok(new { msisdn, isActive });
    }
}
```

### 5. Test the API

```bash
# Get member profile
curl "https://localhost:5001/api/loyalty/member-profile?msisdn=88014123456789"

# Get member tier
curl "https://localhost:5001/api/loyalty/member-tier?msisdn=88014123456789"

# Check if member is active
curl "https://localhost:5001/api/loyalty/is-active?msisdn=88014123456789"
```

## Direct Client Usage (Without Service)

If you prefer using the client directly:

```csharp
public class MyClass
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;

    public MyClass(ILoyaltyClient loyaltyClient, IBanglalinkAuthClient authClient)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
    }

    public async Task GetProfile(string msisdn)
    {
        // Get access token
        var token = await _authClient.GetValidAccessTokenAsync();

        // Create request
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = "LMSMYBLAPP",
            Msisdn = msisdn,
            TransactionID = Guid.NewGuid().ToString()
        };

        // Call API
        var response = await _loyaltyClient.GetMemberProfileAsync(request, token);

        // Access response
        Console.WriteLine($"MSISDN: {response.Msisdn}");
        Console.WriteLine($"Status: {response.StatusMsg}");
        Console.WriteLine($"Points: {response.LoyaltyProfileInfo?.AvailablePoints}");
        Console.WriteLine($"Tier: {response.LoyaltyProfileInfo?.CurrentTierLevel}");
    }
}
```

## Common Response Examples

### Successful Response (200 OK)

```json
{
  "msisdn": "88014123456789",
  "transactionID": "TRX123456",
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

### Error Response

```json
{
  "error": "Member not found or not enrolled in loyalty program"
}
```

## Key Points

✅ **DO:**
- Use `GetValidAccessTokenAsync()` for automatic token management
- Implement proper error handling for each call
- Log all API interactions for debugging
- Validate input (MSISDN format, etc.) before calling API
- Use cancellation tokens for async operations

❌ **DON'T:**
- Hardcode API credentials in code
- Ignore authentication errors
- Call API without error handling
- Make same request repeatedly without caching
- Use HTTP instead of HTTPS

## Supported MSISDN Formats

| Operator | Format | Example |
|----------|--------|---------|
| Banglalink | 88014XXXXXXXXX | 88014123456789 |
| Robi/Airtel | 88018XXXXXXXXX | 88018123456789 |

## Common Status Codes

| Code | Message | Meaning |
|------|---------|---------|
| 0 | SUCCESS | Member profile retrieved successfully |
| 1 | ERROR | General error or member not found |
| 401 | UNAUTHORIZED | Authentication failed |
| 403 | FORBIDDEN | Access denied |
| 500 | INTERNAL_ERROR | Server error |

## Troubleshooting

### Error: "401 Unauthorized"
```
Check your OAuth credentials are correct:
- ClientId
- ClientSecret
- Username
- Password
```

### Error: "Member not found"
```
Ensure:
- MSISDN format is correct (88014XXXXXXXXX)
- Member is enrolled in loyalty program
- Member exists in Banglalink system
```

### Error: "Timeout"
```
Increase timeout in appsettings.json:
"Timeout": "00:00:60"  // 60 seconds
```

## Next Steps

1. **Read Full Documentation:** See [LOYALTY_API_GUIDE.md](LOYALTY_API_GUIDE.md) for comprehensive guide
2. **Explore Examples:** Check `examples/WebApiExample-Net8` for complete working example
3. **Run Tests:** Execute unit tests to verify installation
4. **Implement Caching:** Add caching layer for production use
5. **Monitor Performance:** Add Application Insights monitoring

## Support

- Documentation: [Banglalink API Hub](https://apihub.banglalink.net/)
- Email: api-support@banglalink.net
- GitHub Issues: Report bugs in the project repository

---

**Last Updated:** 2024
**Version:** 1.0.0
