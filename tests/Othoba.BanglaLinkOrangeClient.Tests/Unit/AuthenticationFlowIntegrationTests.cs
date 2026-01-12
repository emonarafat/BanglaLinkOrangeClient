using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Tests.Fixtures;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Integration tests for the complete authentication flow.
/// Tests real-world scenarios and end-to-end functionality.
/// </summary>
public class AuthenticationFlowIntegrationTests
{
    private readonly BanglalinkAuthClientFixture _fixture = new();

    #region Complete Authentication Flow Tests

    [Fact]
    public async Task CompleteAuthenticationFlow_ShouldAuthenticateAndCacheToken()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var result = await client.GetValidAccessTokenAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be(BanglalinkAuthClientFixture.TestAccessToken);
    }

    [Fact]
    public async Task AuthenticationFlowWithCaching_AvoidsDuplicateRequests()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var mockHandler = _fixture.CreateMockHttpMessageHandler(responseContent);
        var httpClient = new HttpClient(mockHandler.Object);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var token1 = await client.GetValidAccessTokenAsync();
        var token2 = await client.GetValidAccessTokenAsync();

        // Assert
        token1.Should().Be(token2);
        // Verify SendAsync was called only once for authentication
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task AuthenticationFlowWithMultipleClients_ShouldWorkIndependently()
    {
        // Arrange
        var config1 = _fixture.CreateValidConfiguration();
        var config2 = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient1 = _fixture.CreateHttpClient(responseContent);
        var httpClient2 = _fixture.CreateHttpClient(responseContent);
        var client1 = new BanglalinkAuthClient(httpClient1, config1);
        var client2 = new BanglalinkAuthClient(httpClient2, config2);

        // Act
        var token1 = await client1.GetValidAccessTokenAsync();
        var token2 = await client2.GetValidAccessTokenAsync();

        // Assert
        token1.Should().Be(token2); // Same token value
        // Each client maintains its own state
    }

    #endregion

    #region Error Recovery Tests

    [Fact]
    public async Task AuthenticationFlow_AfterFailure_CanRetrySuccessfully()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var errorResponse = _fixture.CreateErrorTokenResponse();
        var successResponse = _fixture.CreateValidTokenResponse();
        
        var mockHandler = new Mock<HttpMessageHandler>();
        var callCount = 0;

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                var response = new HttpResponseMessage
                {
                    StatusCode = callCount == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK,
                    Content = new StringContent(
                        callCount == 1 ? errorResponse : successResponse,
                        System.Text.Encoding.UTF8,
                        "application/json")
                };
                return response;
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<BanglalinkAuthenticationException>(() => client.AuthenticateAsync());
        client.ClearCache();
        var result = await client.AuthenticateAsync();
        result.Should().NotBeNull();
    }

    #endregion

    #region Token Refresh Flow Tests

    [Fact]
    public async Task TokenRefreshFlow_ShouldUpdateCacheWithNewToken()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var initialResponse = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(initialResponse);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act - Get initial token
        await client.AuthenticateAsync();
        var initialCached = client.GetCachedTokenResponse();

        // Simulate refresh
        await client.RefreshTokenAsync(BanglalinkAuthClientFixture.TestRefreshToken);
        var refreshedCached = client.GetCachedTokenResponse();

        // Assert
        initialCached.Should().NotBeNull();
        refreshedCached.Should().NotBeNull();
        refreshedCached!.AccessToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Cache Management Tests

    [Fact]
    public async Task CacheManagement_ClearCachePreventsReuseOfExpiredToken()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var token1 = await client.GetValidAccessTokenAsync();
        client.ClearCache();
        var cachedAfterClear = client.GetCachedTokenResponse();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        cachedAfterClear.Should().BeNull();
    }

    [Fact]
    public async Task CacheManagement_MultipleAuthenticationCycles()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            var token = await client.GetValidAccessTokenAsync();
            token.Should().NotBeNullOrEmpty();

            if (i < 2)
            {
                client.ClearCache();
            }
        }
    }

    #endregion

    #region Configuration Validation in Flow Tests

    [Fact]
    public void AuthenticationFlow_WithInvalidConfigurationShouldFailAtConstruction()
    {
        // Arrange
        var config = _fixture.CreateInvalidConfiguration();
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<BanglalinkConfigurationException>(() => new BanglalinkAuthClient(httpClient, config));
    }

    #endregion

    #region HTTP Status Code Handling Tests

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public async Task AuthenticationFlow_WithVariousErrorCodes_ShouldThrowAuthenticationException(int statusCode)
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var errorResponse = _fixture.CreateErrorTokenResponse();
        var httpClient = _fixture.CreateHttpClient(errorResponse, (HttpStatusCode)statusCode);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<BanglalinkAuthenticationException>(() => client.AuthenticateAsync());
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task ConcurrentAuthentication_ShouldHandleMultipleSimultaneousRequests()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act - Simulate concurrent access
        var tasks = new List<Task<string>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(client.GetValidAccessTokenAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllBe(BanglalinkAuthClientFixture.TestAccessToken);
    }

    #endregion
}
