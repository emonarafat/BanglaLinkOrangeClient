# Banglalink Loyalty API - Quick Reference Card

## API Endpoint

```
POST /openapi-lms/loyalty2/get-member-profile
```

**Base URL:** `https://openapi.banglalink.net/`

---

## Request Format

### Headers
```
Authorization: Bearer {access_token}
Content-Type: application/vnd.banglalink.apihub-v1.0+json
```

### Body
```json
{
  "channel": "LMSMYBLAPP",
  "msisdn": "88014123456789",
  "transactionID": "UNIQUE_ID"
}
```

### Parameters
| Field | Type | Required | Example | Notes |
|-------|------|----------|---------|-------|
| channel | string | Yes | LMSMYBLAPP | Channel identifier |
| msisdn | string | Yes | 88014123456789 | 11-digit format |
| transactionID | string | Yes | TRX123456 | Unique per request |

---

## Response Format

### Success (200 OK)
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

### Error (4xx, 5xx)
```json
{
  "statusCode": "1",
  "statusMsg": "ERROR",
  "msisdn": "88014123456789",
  "transactionID": "TRX123456"
}
```

---

## Status Codes

| Code | HTTP | Meaning |
|------|------|---------|
| 0 | 200 | SUCCESS |
| 1 | 400 | ERROR / Not Found |
| 401 | 401 | UNAUTHORIZED |
| 403 | 403 | FORBIDDEN |
| 500 | 500 | INTERNAL_ERROR |

---

## C# Client Usage

### Installation
```bash
dotnet add package Othoba.BanglaLinkOrange
```

### Configuration
```json
{
  "LoyaltyApi": {
    "BaseUrl": "https://openapi.banglalink.net/",
    "Timeout": "00:00:30",
    "RetryCount": 3,
    "RetryDelay": "00:00:00.5"
  }
}
```

### Service Registration
```csharp
var config = new LoyaltyApiConfig();
builder.Configuration.GetSection("LoyaltyApi").Bind(config);
builder.Services.AddBanglalinkLoyaltyClient(config);
```

### Usage (with ILoyaltyService)
```csharp
public class MyClass
{
    private readonly ILoyaltyService _service;
    
    public MyClass(ILoyaltyService service) => _service = service;
    
    public async Task Example()
    {
        var profile = await _service.GetMemberProfileAsync("88014123456789");
        Console.WriteLine(profile.LoyaltyProfileInfo.AvailablePoints);
    }
}
```

### Usage (with ILoyaltyClient)
```csharp
var accessToken = await authClient.GetValidAccessTokenAsync();
var request = new LoyaltyMemberProfileRequest
{
    Channel = "LMSMYBLAPP",
    Msisdn = "88014123456789",
    TransactionID = Guid.NewGuid().ToString()
};
var response = await loyaltyClient.GetMemberProfileAsync(request, accessToken);
```

---

## Key Classes

### LoyaltyMemberProfileRequest
```csharp
public class LoyaltyMemberProfileRequest
{
    public string Channel { get; set; }      // "LMSMYBLAPP"
    public string Msisdn { get; set; }       // "88014XXXXXXXXX"
    public string TransactionID { get; set; } // Unique ID
}
```

### LoyaltyMemberProfileResponse
```csharp
public class LoyaltyMemberProfileResponse
{
    public string Msisdn { get; set; }
    public string TransactionID { get; set; }
    public string StatusCode { get; set; }        // "0" = success
    public string StatusMsg { get; set; }
    public string ResponseDateTime { get; set; }
    public LoyaltyProfileInfo LoyaltyProfileInfo { get; set; }
}
```

### LoyaltyProfileInfo
```csharp
public class LoyaltyProfileInfo
{
    public string AvailablePoints { get; set; }     // Total points
    public string CurrentTierLevel { get; set; }    // e.g., "SIGNATURE"
    public string ExpiryDate { get; set; }          // "DD-MM-YYYY"
    public string PointsExpiring { get; set; }      // Expiring soon
    public string EnrolledDate { get; set; }        // "DD-MM-YYYY HH:MM:SS"
    public string EnrolledChannel { get; set; }     // e.g., "MYBL"
}
```

---

## Error Handling

```csharp
try
{
    var profile = await service.GetMemberProfileAsync(msisdn);
}
catch (ArgumentException ex)
{
    // Invalid input
    Console.WriteLine($"Input error: {ex.Message}");
}
catch (LoyaltyApiException ex)
{
    // API returned error
    Console.WriteLine($"API error: {ex.Message}");
}
catch (BanglalinkAuthenticationException ex)
{
    // Auth failed
    Console.WriteLine($"Auth error: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected error
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## MSISDN Formats

| Operator | Format | Example |
|----------|--------|---------|
| Banglalink | 88014XXXXXXXXX | 88014123456789 |
| Robi/Airtel | 88018XXXXXXXXX | 88018123456789 |

---

## Tier Levels

Common tier levels returned:
- `STANDARD`
- `GOLD`
- `PLATINUM`
- `SIGNATURE`
- `DIAMOND`

---

## Date/Time Formats

**Response DateTime:** `DD-MM-YYYY HH:MM:SS`
- Example: `10-07-2023 14:49:19`

**Expiry Date:** `DD-MM-YYYY`
- Example: `31-12-2024`

**Enrolled Date:** `DD-MM-YYYY HH:MM:SS`
- Example: `21-11-2022 10:31:30`

---

## Common Scenarios

### Get Member Profile
```csharp
var profile = await service.GetMemberProfileAsync("88014123456789");
if (profile.StatusCode == "0")
{
    var points = profile.LoyaltyProfileInfo?.AvailablePoints;
    var tier = profile.LoyaltyProfileInfo?.CurrentTierLevel;
}
```

### Check Member Active Status
```csharp
var isActive = await service.IsMemberActiveAsync("88014123456789");
if (isActive)
{
    Console.WriteLine("Member is enrolled");
}
```

### Get Tier Status
```csharp
var tier = await service.GetMemberTierStatusAsync("88014123456789");
Console.WriteLine($"Tier: {tier.CurrentTier}");
Console.WriteLine($"Days until expiry: {tier.DaysUntilExpiry}");
```

### Get Enriched Profile
```csharp
var enriched = await service.GetEnrichedMemberProfileAsync("88014123456789");
Console.WriteLine($"Active: {enriched.IsActive}");
Console.WriteLine($"Status: {enriched.EnrollmentStatus}");
Console.WriteLine($"Expiring soon: {enriched.HasExpiringPoints}");
```

---

## Troubleshooting

### Problem: 401 Unauthorized
```
Check:
- OAuth credentials (ClientId, ClientSecret)
- Token not expired
- User permissions
```

### Problem: Member Not Found
```
Check:
- MSISDN format (88014XXXXXXXXX)
- Member is enrolled in loyalty program
- Member exists in system
```

### Problem: Timeout
```
Increase timeout in config:
"Timeout": "00:00:60"  // 60 seconds
```

### Problem: Invalid Channel
```
Ensure channel is:
- Registered with Banglalink
- Enabled for Loyalty API
- Correct format: "LMSMYBLAPP"
```

---

## Performance Tips

1. **Cache Results** (5-10 min TTL)
   ```csharp
   _cache.Set($"loyalty:{msisdn}", profile, TimeSpan.FromMinutes(5));
   ```

2. **Use Token Caching**
   ```csharp
   var token = await authClient.GetValidAccessTokenAsync();
   // Token is automatically cached and reused
   ```

3. **Batch Processing**
   ```csharp
   var semaphore = new SemaphoreSlim(5); // Max 5 concurrent
   // Process multiple requests with concurrency limit
   ```

4. **Implement Retry Logic**
   ```csharp
   // Built-in retry configuration:
   "RetryCount": 3,
   "RetryDelay": "00:00:00.5"
   ```

---

## Constants

```csharp
// Default values
const string DEFAULT_CHANNEL = "LMSMYBLAPP";
const string API_BASE_URL = "https://openapi.banglalink.net/";
const string OAUTH_BASE_URL = "https://api.banglalink.net/oauth2/";
const int DEFAULT_TIMEOUT_SECONDS = 30;
const int DEFAULT_RETRY_COUNT = 3;
const int DEFAULT_RETRY_DELAY_MS = 500;

// Status codes
const string STATUS_SUCCESS = "0";
const string STATUS_ERROR = "1";
```

---

## Logging

Enable detailed logging:

```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});
```

Log format:
```
[Timestamp] [Level] [Category] Message
[2024-01-15 10:30:45.123] [INFO] [LoyaltyService] Getting profile for 88014123456789
[2024-01-15 10:30:45.234] [INFO] [LoyaltyClient] POST /openapi-lms/loyalty2/get-member-profile
[2024-01-15 10:30:45.456] [INFO] [LoyaltyService] Successfully retrieved profile
```

---

## Health Check Integration

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("loyalty-api", async () =>
    {
        try
        {
            // Test API connectivity
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    });

app.MapHealthChecks("/health");
```

---

## Related Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | /loyalty/get-member-profile | Get loyalty profile |
| POST | /auth/oauth/token | Get access token |

---

## Authentication

### Get Access Token
```csharp
var authClient = new BanglalinkAuthClient2(
    "https://api.banglalink.net/oauth2/",
    "clientId",
    "clientSecret",
    logger);

var token = await authClient.GetValidAccessTokenAsync();
```

**Note:** Token is automatically cached and refreshed

---

## Rate Limiting

Recommended approach:
- Max 100 requests per minute per channel
- Implement exponential backoff for retries
- Cache responses appropriately

---

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("LoyaltyApi", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("LoyaltyApi");
```

---

## Dependencies

```xml
<ItemGroup>
    <PackageReference Include="Othoba.BanglaLinkOrange" Version="1.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />
    <PackageReference Include="System.Text.Json" Version="4.7.2" />
</ItemGroup>
```

---

## Links

- API Hub: https://apihub.banglalink.net/
- Quick Start: [LOYALTY_QUICK_START.md](LOYALTY_QUICK_START.md)
- Full Guide: [LOYALTY_API_GUIDE.md](LOYALTY_API_GUIDE.md)
- Examples: [LOYALTY_API_EXAMPLES.md](LOYALTY_API_EXAMPLES.md)

---

**Last Updated:** 2024 | **Version:** 1.0.0 | **Status:** Production Ready
