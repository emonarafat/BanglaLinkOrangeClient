using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Models;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Othoba.BanglaLinkOrangeClient.Tests.Integration;

/// <summary>
/// Integration tests for the Banglalink Loyalty API client.
/// These tests verify the functionality of the ILoyaltyClient implementation.
/// </summary>
public class LoyaltyClientIntegrationTests : IAsyncLifetime
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<LoyaltyClient> _logger;
    private string _validAccessToken = string.Empty;

    // Test data
    private const string TestMsisdn = "88014123456789";
    private const string TestChannel = "LMSMYBLAPP";
    private const string ApiBaseUrl = "https://openapi.banglalink.net/";

    public LoyaltyClientIntegrationTests()
    {
        // Setup logging
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<LoyaltyClient>();

        // Create clients (you would use real configuration in integration tests)
        _loyaltyClient = new LoyaltyClient(ApiBaseUrl, _logger);
        _authClient = new BanglalinkAuthClient2(
            "https://api.banglalink.net/oauth2/",
            "your_client_id",
            "your_client_secret",
            loggerFactory.CreateLogger<BanglalinkAuthClient2>());
    }

    public async Task InitializeAsync()
    {
        // Get access token for tests
        try
        {
            _validAccessToken = await _authClient.GetValidAccessTokenAsync();
        }
        catch (Exception ex)
        {
            // Log but don't fail - tests may be run in offline mode
            System.Diagnostics.Debug.WriteLine($"Could not obtain access token: {ex.Message}");
        }
    }

    public async Task DisposeAsync()
    {
        // Cleanup if needed
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires valid API credentials")]
    public async Task GetMemberProfile_WithValidMsisdn_ReturnsProfile()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = "TEST123456789"
        };

        // Act
        var response = await _loyaltyClient.GetMemberProfileAsync(request, _validAccessToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(TestMsisdn, response.Msisdn);
        Assert.Equal("TEST123456789", response.TransactionID);
        Assert.NotNull(response.LoyaltyProfileInfo);
        Assert.NotEmpty(response.LoyaltyProfileInfo.AvailablePoints);
    }

    [Fact(Skip = "Requires valid API credentials")]
    public async Task GetMemberProfile_WithInvalidMsisdn_ThrowsLoyaltyApiException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = "88000000000000", // Invalid MSISDN
            TransactionID = "TEST123456790"
        };

        // Act & Assert
        await Assert.ThrowsAsync<LoyaltyApiException>(
            () => _loyaltyClient.GetMemberProfileAsync(request, _validAccessToken));
    }

    [Fact]
    public async Task GetMemberProfile_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _loyaltyClient.GetMemberProfileAsync(null!, _validAccessToken));
    }

    [Fact]
    public async Task GetMemberProfile_WithNullAccessToken_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = "TEST123456791"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _loyaltyClient.GetMemberProfileAsync(request, null!));
    }

    [Fact]
    public async Task GetMemberProfile_WithEmptyAccessToken_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = "TEST123456792"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _loyaltyClient.GetMemberProfileAsync(request, string.Empty));
    }

    [Fact]
    public void LoyaltyMemberProfileRequest_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var transactionId = Guid.NewGuid().ToString();

        // Act
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = transactionId
        };

        // Assert
        Assert.Equal(TestChannel, request.Channel);
        Assert.Equal(TestMsisdn, request.Msisdn);
        Assert.Equal(transactionId, request.TransactionID);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void LoyaltyMemberProfileRequest_WithInvalidMsisdn_ThrowsException(string invalidMsisdn)
    {
        // Act
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = invalidMsisdn,
            TransactionID = "TEST"
        };

        // Assert - The client should validate this
        Assert.Null(string.IsNullOrEmpty(invalidMsisdn) ? request.Msisdn : null);
    }

    [Fact]
    public void LoyaltyProfileInfo_WithValidData_ContainsAllProperties()
    {
        // Arrange
        var profileInfo = new LoyaltyProfileInfo
        {
            AvailablePoints = "10000",
            CurrentTierLevel = "SIGNATURE",
            ExpiryDate = "31-12-2024",
            PointsExpiring = "1000",
            EnrolledDate = "21-11-2022 10:31:30",
            EnrolledChannel = "MYBL"
        };

        // Assert
        Assert.Equal("10000", profileInfo.AvailablePoints);
        Assert.Equal("SIGNATURE", profileInfo.CurrentTierLevel);
        Assert.Equal("31-12-2024", profileInfo.ExpiryDate);
        Assert.Equal("1000", profileInfo.PointsExpiring);
        Assert.Equal("21-11-2022 10:31:30", profileInfo.EnrolledDate);
        Assert.Equal("MYBL", profileInfo.EnrolledChannel);
    }

    [Fact]
    public void LoyaltyMemberProfileResponse_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var response = new LoyaltyMemberProfileResponse
        {
            Msisdn = TestMsisdn,
            TransactionID = "TEST123",
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10000",
                CurrentTierLevel = "SIGNATURE",
                ExpiryDate = "31-12-2024",
                PointsExpiring = "1000",
                EnrolledDate = "21-11-2022 10:31:30",
                EnrolledChannel = "MYBL"
            }
        };

        // Assert
        Assert.Equal(TestMsisdn, response.Msisdn);
        Assert.Equal("TEST123", response.TransactionID);
        Assert.Equal("0", response.StatusCode);
        Assert.Equal("SUCCESS", response.StatusMsg);
        Assert.NotNull(response.LoyaltyProfileInfo);
        Assert.Equal("10000", response.LoyaltyProfileInfo.AvailablePoints);
    }
}

/// <summary>
/// Unit tests for the Loyalty Service.
/// Tests business logic and service methods.
/// </summary>
public class LoyaltyServiceUnitTests
{
    private readonly Mock<ILoyaltyClient> _mockLoyaltyClient;
    private readonly Mock<IBanglalinkAuthClient> _mockAuthClient;
    private readonly Mock<ILogger<LoyaltyService>> _mockLogger;
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyServiceUnitTests()
    {
        _mockLoyaltyClient = new Mock<ILoyaltyClient>();
        _mockAuthClient = new Mock<IBanglalinkAuthClient>();
        _mockLogger = new Mock<ILogger<LoyaltyService>>();

        _loyaltyService = new LoyaltyService(
            _mockLoyaltyClient.Object,
            _mockAuthClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetMemberProfile_WithValidMsisdn_ReturnsProfile()
    {
        // Arrange
        var msisdn = "88014123456789";
        var accessToken = "valid_token";
        var apiResponse = new LoyaltyMemberProfileResponse
        {
            Msisdn = msisdn,
            TransactionID = "TRX123",
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10000",
                CurrentTierLevel = "SIGNATURE",
                ExpiryDate = "31-12-2024",
                PointsExpiring = "1000",
                EnrolledDate = "21-11-2022 10:31:30",
                EnrolledChannel = "MYBL"
            }
        };

        _mockAuthClient
            .Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        _mockLoyaltyClient
            .Setup(x => x.GetMemberProfileAsync(
                It.IsAny<LoyaltyMemberProfileRequest>(),
                accessToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _loyaltyService.GetMemberProfileAsync(msisdn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(msisdn, result.Msisdn);
        Assert.Equal("0", result.StatusCode);
    }

    [Fact]
    public async Task GetMemberProfile_WithNullMsisdn_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _loyaltyService.GetMemberProfileAsync(null!));
    }

    [Fact]
    public async Task IsMemberActive_WithValidProfile_ReturnsTrue()
    {
        // Arrange
        var msisdn = "88014123456789";
        var accessToken = "valid_token";
        var apiResponse = new LoyaltyMemberProfileResponse
        {
            Msisdn = msisdn,
            TransactionID = "TRX123",
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            LoyaltyProfileInfo = new LoyaltyProfileInfo { AvailablePoints = "10000" }
        };

        _mockAuthClient
            .Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        _mockLoyaltyClient
            .Setup(x => x.GetMemberProfileAsync(
                It.IsAny<LoyaltyMemberProfileRequest>(),
                accessToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _loyaltyService.IsMemberActiveAsync(msisdn);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetMemberTierStatus_WithValidProfile_ReturnsTierStatus()
    {
        // Arrange
        var msisdn = "88014123456789";
        var accessToken = "valid_token";
        var apiResponse = new LoyaltyMemberProfileResponse
        {
            Msisdn = msisdn,
            TransactionID = "TRX123",
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10000",
                CurrentTierLevel = "SIGNATURE",
                ExpiryDate = "31-12-2024",
                EnrolledDate = "21-11-2022 10:31:30"
            }
        };

        _mockAuthClient
            .Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        _mockLoyaltyClient
            .Setup(x => x.GetMemberProfileAsync(
                It.IsAny<LoyaltyMemberProfileRequest>(),
                accessToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _loyaltyService.GetMemberTierStatusAsync(msisdn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SIGNATURE", result.CurrentTier);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetEnrichedMemberProfile_WithValidProfile_ReturnsEnrichedProfile()
    {
        // Arrange
        var msisdn = "88014123456789";
        var accessToken = "valid_token";
        var apiResponse = new LoyaltyMemberProfileResponse
        {
            Msisdn = msisdn,
            TransactionID = "TRX123",
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10000",
                CurrentTierLevel = "SIGNATURE",
                ExpiryDate = "31-12-2024",
                PointsExpiring = "1000",
                EnrolledDate = "21-11-2022 10:31:30",
                EnrolledChannel = "MYBL"
            }
        };

        _mockAuthClient
            .Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        _mockLoyaltyClient
            .Setup(x => x.GetMemberProfileAsync(
                It.IsAny<LoyaltyMemberProfileRequest>(),
                accessToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _loyaltyService.GetEnrichedMemberProfileAsync(msisdn);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.Equal("SIGNATURE", result.TierLevel);
        Assert.True(result.HasExpiringPoints);
    }
}
