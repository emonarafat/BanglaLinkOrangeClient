# Banglalink Loyalty API - Usage Examples

## Table of Contents
1. [Basic Usage](#basic-usage)
2. [Web API Controller Examples](#web-api-controller-examples)
3. [Service Layer Examples](#service-layer-examples)
4. [Advanced Scenarios](#advanced-scenarios)
5. [Error Handling Examples](#error-handling-examples)
6. [Testing Examples](#testing-examples)

## Basic Usage

### Simple Console Application

```csharp
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Models;
using Microsoft.Extensions.Logging;

// Setup logging
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var authLogger = loggerFactory.CreateLogger<BanglalinkAuthClient2>();
var loyaltyLogger = loggerFactory.CreateLogger<LoyaltyClient>();

// Create clients
var authClient = new BanglalinkAuthClient2(
    baseUrl: "https://api.banglalink.net/oauth2/",
    clientId: "your_client_id",
    clientSecret: "your_client_secret",
    logger: authLogger);

var loyaltyClient = new LoyaltyClient(
    baseUrl: "https://openapi.banglalink.net/",
    logger: loyaltyLogger);

try
{
    // Get access token
    var accessToken = await authClient.GetValidAccessTokenAsync();
    Console.WriteLine("Token obtained successfully");

    // Create request
    var request = new LoyaltyMemberProfileRequest
    {
        Channel = "LMSMYBLAPP",
        Msisdn = "88014123456789",
        TransactionID = $"TRX{DateTime.Now.Ticks}"
    };

    // Get member profile
    var response = await loyaltyClient.GetMemberProfileAsync(request, accessToken);

    // Display results
    if (response.StatusCode == "0")
    {
        Console.WriteLine($"MSISDN: {response.Msisdn}");
        Console.WriteLine($"Status: {response.StatusMsg}");
        
        if (response.LoyaltyProfileInfo != null)
        {
            Console.WriteLine($"Available Points: {response.LoyaltyProfileInfo.AvailablePoints}");
            Console.WriteLine($"Tier Level: {response.LoyaltyProfileInfo.CurrentTierLevel}");
            Console.WriteLine($"Tier Expiry: {response.LoyaltyProfileInfo.ExpiryDate}");
            Console.WriteLine($"Enrolled: {response.LoyaltyProfileInfo.EnrolledDate}");
        }
    }
    else
    {
        Console.WriteLine($"Error: {response.StatusMsg}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}
```

## Web API Controller Examples

### Example 1: Simple Member Profile Endpoint

```csharp
using Microsoft.AspNetCore.Mvc;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Models;

[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<LoyaltyController> _logger;

    public LoyaltyController(
        ILoyaltyClient loyaltyClient,
        IBanglalinkAuthClient authClient,
        ILogger<LoyaltyController> logger)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets member loyalty profile information
    /// </summary>
    /// <param name="msisdn">Customer MSISDN (88014XXXXXXXXX)</param>
    /// <returns>Member profile with loyalty details</returns>
    [HttpGet("profile")]
    [Produces("application/json")]
    [ProduceResponseType(typeof(LoyaltyMemberProfileResponse), 200)]
    [ProduceResponseType(400)]
    [ProduceResponseType(404)]
    [ProduceResponseType(500)]
    public async Task<IActionResult> GetProfile([FromQuery] string msisdn)
    {
        if (string.IsNullOrWhiteSpace(msisdn))
            return BadRequest(new { error = "MSISDN is required" });

        try
        {
            var accessToken = await _authClient.GetValidAccessTokenAsync();
            
            var request = new LoyaltyMemberProfileRequest
            {
                Channel = "LMSMYBLAPP",
                Msisdn = msisdn,
                TransactionID = Guid.NewGuid().ToString()
            };

            var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for {Msisdn}", msisdn);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}
```

### Example 2: Endpoint with Batch Processing

```csharp
[HttpPost("profiles/batch")]
public async Task<IActionResult> GetMultipleProfiles(
    [FromBody] BatchMemberProfileRequest request,
    CancellationToken cancellationToken)
{
    if (request?.Msisdns == null || !request.Msisdns.Any())
        return BadRequest(new { error = "At least one MSISDN is required" });

    try
    {
        var accessToken = await _authClient.GetValidAccessTokenAsync(cancellationToken);
        var results = new List<MemberProfileResult>();

        // Process in parallel with concurrency limit
        var semaphore = new SemaphoreSlim(5); // Max 5 concurrent requests
        var tasks = request.Msisdns.Select(async msisdn =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var loyaltyRequest = new LoyaltyMemberProfileRequest
                {
                    Channel = request.Channel ?? "LMSMYBLAPP",
                    Msisdn = msisdn,
                    TransactionID = Guid.NewGuid().ToString()
                };

                var response = await _loyaltyClient.GetMemberProfileAsync(
                    loyaltyRequest, accessToken, cancellationToken);

                return new MemberProfileResult
                {
                    Msisdn = msisdn,
                    Success = response.StatusCode == "0",
                    Data = response,
                    Error = response.StatusMsg
                };
            }
            catch (Exception ex)
            {
                return new MemberProfileResult
                {
                    Msisdn = msisdn,
                    Success = false,
                    Error = ex.Message
                };
            }
            finally
            {
                semaphore.Release();
            }
        });

        var batchResults = await Task.WhenAll(tasks);
        return Ok(new { total = batchResults.Length, results = batchResults });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing batch request");
        return StatusCode(500, new { error = "Batch processing failed" });
    }
}
```

### Example 3: Endpoint with Caching

```csharp
public class CachedLoyaltyController : ControllerBase
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "loyalty:profile:";
    private const int CacheDurationMinutes = 5;

    public CachedLoyaltyController(
        ILoyaltyClient loyaltyClient,
        IBanglalinkAuthClient authClient,
        IMemoryCache cache)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
        _cache = cache;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile([FromQuery] string msisdn)
    {
        var cacheKey = $"{CacheKeyPrefix}{msisdn}";

        if (_cache.TryGetValue(cacheKey, out LoyaltyMemberProfileResponse cached))
        {
            return Ok(new { data = cached, source = "cache" });
        }

        try
        {
            var accessToken = await _authClient.GetValidAccessTokenAsync();
            
            var request = new LoyaltyMemberProfileRequest
            {
                Channel = "LMSMYBLAPP",
                Msisdn = msisdn,
                TransactionID = Guid.NewGuid().ToString()
            };

            var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken);

            // Cache successful responses only
            if (response.StatusCode == "0")
            {
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return Ok(new { data = response, source = "api" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
```

## Service Layer Examples

### Example 1: Basic Service

```csharp
public interface ILoyaltyService
{
    Task<LoyaltyProfile> GetMemberProfileAsync(string msisdn);
}

public class LoyaltyService : ILoyaltyService
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<LoyaltyService> _logger;

    public LoyaltyService(
        ILoyaltyClient loyaltyClient,
        IBanglalinkAuthClient authClient,
        ILogger<LoyaltyService> logger)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
        _logger = logger;
    }

    public async Task<LoyaltyProfile> GetMemberProfileAsync(string msisdn)
    {
        if (string.IsNullOrWhiteSpace(msisdn))
            throw new ArgumentException("MSISDN is required");

        try
        {
            var accessToken = await _authClient.GetValidAccessTokenAsync();
            
            var request = new LoyaltyMemberProfileRequest
            {
                Channel = "LMSMYBLAPP",
                Msisdn = msisdn,
                TransactionID = Guid.NewGuid().ToString()
            };

            var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken);

            return new LoyaltyProfile
            {
                Msisdn = response.Msisdn,
                IsActive = response.StatusCode == "0",
                AvailablePoints = response.LoyaltyProfileInfo?.AvailablePoints,
                TierLevel = response.LoyaltyProfileInfo?.CurrentTierLevel,
                TierExpiryDate = response.LoyaltyProfileInfo?.ExpiryDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty profile for {Msisdn}", msisdn);
            throw;
        }
    }
}

public class LoyaltyProfile
{
    public string Msisdn { get; set; }
    public bool IsActive { get; set; }
    public string AvailablePoints { get; set; }
    public string TierLevel { get; set; }
    public string TierExpiryDate { get; set; }
}
```

### Example 2: Service with Business Logic

```csharp
public interface ILoyaltyManagementService
{
    Task<MemberTierStatus> GetTierStatusAsync(string msisdn);
    Task<bool> IsMemberEligibleForPromotionAsync(string msisdn);
    Task<PointsAnalysis> AnalyzeMemberPointsAsync(string msisdn);
}

public class LoyaltyManagementService : ILoyaltyManagementService
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<LoyaltyManagementService> _logger;

    public async Task<MemberTierStatus> GetTierStatusAsync(string msisdn)
    {
        var accessToken = await _authClient.GetValidAccessTokenAsync();
        
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = "LMSMYBLAPP",
            Msisdn = msisdn,
            TransactionID = Guid.NewGuid().ToString()
        };

        var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken);
        
        if (response.LoyaltyProfileInfo == null)
            throw new InvalidOperationException("Member profile not found");

        var expiryDate = DateTime.ParseExact(
            response.LoyaltyProfileInfo.ExpiryDate,
            "dd-MM-yyyy",
            CultureInfo.InvariantCulture);

        var daysUntilExpiry = (expiryDate - DateTime.Now).Days;

        return new MemberTierStatus
        {
            CurrentTier = response.LoyaltyProfileInfo.CurrentTierLevel,
            DaysUntilExpiry = daysUntilExpiry,
            IsExpiringSoon = daysUntilExpiry <= 30,
            AvailablePoints = long.Parse(response.LoyaltyProfileInfo.AvailablePoints),
            ExpiringPoints = long.Parse(response.LoyaltyProfileInfo.PointsExpiring)
        };
    }

    public async Task<bool> IsMemberEligibleForPromotionAsync(string msisdn)
    {
        var tierStatus = await GetTierStatusAsync(msisdn);
        
        // Eligible if: member has points, tier not expiring soon
        return tierStatus.AvailablePoints > 0 && !tierStatus.IsExpiringSoon;
    }

    public async Task<PointsAnalysis> AnalyzeMemberPointsAsync(string msisdn)
    {
        var tierStatus = await GetTierStatusAsync(msisdn);

        return new PointsAnalysis
        {
            TotalPoints = tierStatus.AvailablePoints,
            ExpiringPoints = tierStatus.ExpiringPoints,
            PointsHealthPercentage = tierStatus.ExpiringPoints == 0 
                ? 100 
                : (int)((tierStatus.AvailablePoints - tierStatus.ExpiringPoints) 
                    / (double)tierStatus.AvailablePoints * 100),
            RecommendedAction = GetRecommendedAction(tierStatus)
        };
    }

    private string GetRecommendedAction(MemberTierStatus tierStatus)
    {
        if (tierStatus.IsExpiringSoon)
            return "Renew tier membership";
        
        if (tierStatus.ExpiringPoints > 0)
            return "Redeem expiring points";
        
        return "Continue enjoying rewards";
    }
}

public class MemberTierStatus
{
    public string CurrentTier { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsExpiringSoon { get; set; }
    public long AvailablePoints { get; set; }
    public long ExpiringPoints { get; set; }
}

public class PointsAnalysis
{
    public long TotalPoints { get; set; }
    public long ExpiringPoints { get; set; }
    public int PointsHealthPercentage { get; set; }
    public string RecommendedAction { get; set; }
}
```

## Advanced Scenarios

### Scenario 1: Retry Logic with Exponential Backoff

```csharp
public async Task<LoyaltyMemberProfileResponse> GetMemberProfileWithRetryAsync(
    string msisdn,
    int maxRetries = 3,
    int delayMs = 1000)
{
    var accessToken = await _authClient.GetValidAccessTokenAsync();
    
    var request = new LoyaltyMemberProfileRequest
    {
        Channel = "LMSMYBLAPP",
        Msisdn = msisdn,
        TransactionID = Guid.NewGuid().ToString()
    };

    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        try
        {
            return await _loyaltyClient.GetMemberProfileAsync(request, accessToken);
        }
        catch (HttpRequestException) when (attempt < maxRetries - 1)
        {
            var delay = delayMs * (int)Math.Pow(2, attempt); // Exponential backoff
            _logger.LogWarning("Attempt {Attempt} failed, retrying in {Delay}ms", 
                attempt + 1, delay);
            await Task.Delay(delay);
        }
    }

    // This should not reach if all retries succeed
    throw new InvalidOperationException("All retry attempts failed");
}
```

### Scenario 2: Queue-based Processing

```csharp
public class LoyaltyProfileQueueProcessor
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly Channel<string> _channel;
    private readonly ILogger<LoyaltyProfileQueueProcessor> _logger;

    public async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        var accessToken = await _authClient.GetValidAccessTokenAsync(cancellationToken);

        await foreach (var msisdn in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                var request = new LoyaltyMemberProfileRequest
                {
                    Channel = "LMSMYBLAPP",
                    Msisdn = msisdn,
                    TransactionID = Guid.NewGuid().ToString()
                };

                var response = await _loyaltyClient.GetMemberProfileAsync(
                    request, accessToken, cancellationToken);

                _logger.LogInformation("Processed {Msisdn}: {Status}", 
                    msisdn, response.StatusMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {Msisdn}", msisdn);
            }
        }
    }

    public async Task EnqueueAsync(string msisdn)
    {
        await _channel.Writer.WriteAsync(msisdn);
    }

    public void CompleteQueue()
    {
        _channel.Writer.Complete();
    }
}
```

## Error Handling Examples

### Comprehensive Error Handling

```csharp
[HttpGet("profile")]
public async Task<IActionResult> GetProfile([FromQuery] string msisdn)
{
    try
    {
        if (string.IsNullOrWhiteSpace(msisdn))
            return BadRequest(CreateErrorResponse("MSISDN_REQUIRED", "MSISDN is required"));

        if (!IsValidMsisdn(msisdn))
            return BadRequest(CreateErrorResponse("INVALID_MSISDN", "Invalid MSISDN format"));

        var profile = await _loyaltyService.GetMemberProfileAsync(msisdn);
        return Ok(profile);
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
        return BadRequest(CreateErrorResponse("VALIDATION_ERROR", ex.Message));
    }
    catch (LoyaltyApiException ex)
    {
        _logger.LogError(ex, "API error: {Message}", ex.Message);
        
        return ex.StatusCode switch
        {
            404 => NotFound(CreateErrorResponse("MEMBER_NOT_FOUND", "Member not found")),
            401 => Unauthorized(CreateErrorResponse("AUTH_FAILED", "Authentication failed")),
            403 => Forbid(),
            _ => StatusCode(500, CreateErrorResponse("API_ERROR", ex.Message))
        };
    }
    catch (BanglalinkAuthenticationException ex)
    {
        _logger.LogError(ex, "Authentication failed: {Message}", ex.Message);
        return StatusCode(401, CreateErrorResponse("AUTH_ERROR", "Authentication failed"));
    }
    catch (TimeoutException ex)
    {
        _logger.LogError(ex, "Request timeout");
        return StatusCode(504, CreateErrorResponse("TIMEOUT", "Request timeout"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error");
        return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An unexpected error occurred"));
    }
}

private ErrorResponse CreateErrorResponse(string code, string message)
{
    return new ErrorResponse
    {
        Code = code,
        Message = message,
        Timestamp = DateTime.UtcNow
    };
}

public class ErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Testing Examples

### Unit Tests with Mocks

```csharp
public class LoyaltyServiceTests : IDisposable
{
    private readonly Mock<ILoyaltyClient> _mockClient;
    private readonly Mock<IBanglalinkAuthClient> _mockAuthClient;
    private readonly Mock<ILogger<LoyaltyService>> _mockLogger;
    private readonly LoyaltyService _service;

    public LoyaltyServiceTests()
    {
        _mockClient = new Mock<ILoyaltyClient>();
        _mockAuthClient = new Mock<IBanglalinkAuthClient>();
        _mockLogger = new Mock<ILogger<LoyaltyService>>();

        _service = new LoyaltyService(_mockClient.Object, _mockAuthClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetMemberProfile_WithValidMsisdn_ReturnsProfile()
    {
        // Arrange
        var msisdn = "88014123456789";
        var response = new LoyaltyMemberProfileResponse
        {
            Msisdn = msisdn,
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10000",
                CurrentTierLevel = "SIGNATURE"
            }
        };

        _mockAuthClient
            .Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("token");

        _mockClient
            .Setup(x => x.GetMemberProfileAsync(
                It.IsAny<LoyaltyMemberProfileRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetMemberProfileAsync(msisdn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(msisdn, result.Msisdn);
        _mockClient.Verify(x => x.GetMemberProfileAsync(
            It.IsAny<LoyaltyMemberProfileRequest>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _mockClient.Reset();
        _mockAuthClient.Reset();
    }
}
```

---

For more examples and detailed usage patterns, refer to the complete documentation in [LOYALTY_API_GUIDE.md](LOYALTY_API_GUIDE.md).
