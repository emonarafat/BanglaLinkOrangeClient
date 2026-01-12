using FluentAssertions;
using Moq;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Models;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for LoyaltyClient class.
/// Tests member profile retrieval, error handling, and request validation.
/// </summary>
public class LoyaltyClientTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LoyaltyClientConfiguration _configuration;
    private readonly Mock<IBanglalinkAuthClient> _authClientMock;

    private const string TestAccessToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJoWUxNdzlVVkF1THpkYVNwUjZiRG00ckpkNE1ETnZEeXV1cFhhNlZYN3BZIn0";
    private const string TestMsisdn = "88014########";
    private const string TestTransactionId = "LMS34197492";
    private const string TestChannel = "LMSMYBLAPP";

    public LoyaltyClientTests()
    {
        _httpClient = new HttpClient();
        _configuration = new LoyaltyClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            GetMemberProfileEndpoint = "/openapi-lms/loyalty2/get-member-profile",
            ContentType = "application/vnd.banglalink.apihub-v1.0+json",
            RequestTimeoutSeconds = 30
        };
        _authClientMock = new Mock<IBanglalinkAuthClient>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitialize()
    {
        // Act
        var client = new LoyaltyClient(_httpClient, _configuration);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LoyaltyClient(null!, _configuration));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LoyaltyClient(_httpClient, null!));
    }

    #endregion

    #region GetMemberProfileAsync with AccessToken Tests

    [Fact]
    public async Task GetMemberProfileAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var mockResponse = new LoyaltyMemberProfileResponse
        {
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId,
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = "10-07-2023 14:49:19",
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10409080",
                CurrentTierLevel = "SIGNATURE",
                ExpiryDate = "31-12-2024",
                PointsExpiring = "10409080",
                EnrolledDate = "21-11-2022 10:31:30",
                EnrolledChannel = ""
            }
        };

        var mockHandler = new MockHttpMessageHandler(mockResponse);
        var client = new LoyaltyClient(new HttpClient(mockHandler), _configuration);

        // Act
        var result = await client.GetMemberProfileAsync(request, TestAccessToken);

        // Assert
        result.Should().NotBeNull();
        result.Msisdn.Should().Be(TestMsisdn);
        result.StatusCode.Should().Be("0");
        result.IsSuccessful.Should().BeTrue();
        result.LoyaltyProfileInfo.Should().NotBeNull();
        result.LoyaltyProfileInfo!.AvailablePoints.Should().Be("10409080");
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new LoyaltyClient(_httpClient, _configuration).GetMemberProfileAsync(null!, TestAccessToken));
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithNullAccessToken_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new LoyaltyClient(_httpClient, _configuration).GetMemberProfileAsync(request, null!));
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithEmptyAccessToken_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new LoyaltyClient(_httpClient, _configuration).GetMemberProfileAsync(request, ""));
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithEmptyMsisdn_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = "",
            TransactionID = TestTransactionId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new LoyaltyClient(_httpClient, _configuration).GetMemberProfileAsync(request, TestAccessToken));
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithEmptyChannel_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = "",
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new LoyaltyClient(_httpClient, _configuration).GetMemberProfileAsync(request, TestAccessToken));
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithEmptyTransactionId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new LoyaltyClient(_httpClient, _configuration).GetMemberProfileAsync(request, TestAccessToken));
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithUnauthorizedResponse_ShouldThrowLoyaltyApiException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.Unauthorized);
        var client = new LoyaltyClient(new HttpClient(mockHandler), _configuration);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LoyaltyApiException>(() =>
            client.GetMemberProfileAsync(request, TestAccessToken));

        ex.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithForbiddenResponse_ShouldThrowLoyaltyApiException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.Forbidden);
        var client = new LoyaltyClient(new HttpClient(mockHandler), _configuration);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LoyaltyApiException>(() =>
            client.GetMemberProfileAsync(request, TestAccessToken));

        ex.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithBadRequestResponse_ShouldThrowLoyaltyApiException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.BadRequest);
        var client = new LoyaltyClient(new HttpClient(mockHandler), _configuration);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LoyaltyApiException>(() =>
            client.GetMemberProfileAsync(request, TestAccessToken));

        ex.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithNotFoundResponse_ShouldThrowLoyaltyApiException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.NotFound);
        var client = new LoyaltyClient(new HttpClient(mockHandler), _configuration);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LoyaltyApiException>(() =>
            client.GetMemberProfileAsync(request, TestAccessToken));

        ex.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetMemberProfileAsync with Auth Client Tests

    [Fact]
    public async Task GetMemberProfileAsync_WithAuthClient_ShouldUseProvidedToken()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var mockResponse = new LoyaltyMemberProfileResponse
        {
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId,
            StatusCode = "0",
            StatusMsg = "SUCCESS",
            ResponseDateTime = "10-07-2023 14:49:19",
            LoyaltyProfileInfo = new LoyaltyProfileInfo
            {
                AvailablePoints = "10409080",
                CurrentTierLevel = "SIGNATURE",
                ExpiryDate = "31-12-2024",
                PointsExpiring = "10409080",
                EnrolledDate = "21-11-2022 10:31:30",
                EnrolledChannel = ""
            }
        };

        _authClientMock.Setup(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestAccessToken);

        var mockHandler = new MockHttpMessageHandler(mockResponse);
        var client = new LoyaltyClient(new HttpClient(mockHandler), _configuration, _authClientMock.Object);

        // Act
        var result = await client.GetMemberProfileAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        _authClientMock.Verify(x => x.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMemberProfileAsync_WithoutAuthClient_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new LoyaltyMemberProfileRequest
        {
            Channel = TestChannel,
            Msisdn = TestMsisdn,
            TransactionID = TestTransactionId
        };

        var client = new LoyaltyClient(_httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetMemberProfileAsync(request));
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public void LoyaltyMemberProfileResponse_WithSuccessStatus_ShouldIndicateSuccess()
    {
        // Arrange & Act
        var response = new LoyaltyMemberProfileResponse
        {
            StatusCode = "0",
            StatusMsg = "SUCCESS"
        };

        // Assert
        response.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void LoyaltyMemberProfileResponse_WithFailureStatus_ShouldIndicateFailure()
    {
        // Arrange & Act
        var response = new LoyaltyMemberProfileResponse
        {
            StatusCode = "1",
            StatusMsg = "FAILURE"
        };

        // Assert
        response.IsSuccessful.Should().BeFalse();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void LoyaltyClientConfiguration_WithValidSettings_ShouldBeValid()
    {
        // Act
        var isValid = _configuration.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void LoyaltyClientConfiguration_WithEmptyBaseUrl_ShouldBeInvalid()
    {
        // Arrange
        _configuration.BaseUrl = "";

        // Act
        var isValid = _configuration.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void LoyaltyClientConfiguration_GetMemberProfileUrl_ShouldCombineBaseUrlAndEndpoint()
    {
        // Act
        var url = _configuration.GetMemberProfileUrl;

        // Assert
        url.Should().Be("http://localhost:8080/openapi-lms/loyalty2/get-member-profile");
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Mock HTTP message handler for testing.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly LoyaltyMemberProfileResponse? _response;
    private readonly System.Net.HttpStatusCode _statusCode;

    public MockHttpMessageHandler(LoyaltyMemberProfileResponse response)
    {
        _response = response;
        _statusCode = System.Net.HttpStatusCode.OK;
    }

    public MockHttpMessageHandler(System.Net.HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        _response = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode);

        if (_response != null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_response, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        return await Task.FromResult(response);
    }
}
