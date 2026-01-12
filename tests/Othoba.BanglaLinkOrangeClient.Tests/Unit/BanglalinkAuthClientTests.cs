using System.Net;
using FluentAssertions;
using Moq;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Tests.Fixtures;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for BanglalinkAuthClient class.
/// Tests authentication flows, token caching, and error handling.
/// </summary>
public class BanglalinkAuthClientTests
{
    private readonly BanglalinkAuthClientFixture _fixture = new();

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldInitialize()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var httpClient = new HttpClient();

        // Act
        var client = new BanglalinkAuthClient(httpClient, config);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        HttpClient? httpClient = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BanglalinkAuthClient(httpClient!, config));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BanglalinkAuthClient(httpClient, null!));
    }

    [Fact]
    public void Constructor_WithInvalidConfiguration_ShouldThrowBanglalinkConfigurationException()
    {
        // Arrange
        var config = _fixture.CreateInvalidConfiguration();
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<BanglalinkConfigurationException>(() => new BanglalinkAuthClient(httpClient, config));
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnTokenResponse()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var result = await client.AuthenticateAsync();

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().Be(BanglalinkAuthClientFixture.TestAccessToken);
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(36000);
        result.RefreshToken.Should().Be(BanglalinkAuthClientFixture.TestRefreshToken);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldThrowBanglalinkAuthenticationException()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateErrorTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent, HttpStatusCode.Unauthorized);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<BanglalinkAuthenticationException>(() => client.AuthenticateAsync());
    }

    [Fact]
    public async Task AuthenticateAsync_CachesToken()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        await client.AuthenticateAsync();
        var cachedToken = client.GetCachedTokenResponse();

        // Assert
        cachedToken.Should().NotBeNull();
        cachedToken!.AccessToken.Should().Be(BanglalinkAuthClientFixture.TestAccessToken);
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ShouldReturnNewTokenResponse()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);
        var refreshToken = BanglalinkAuthClientFixture.TestRefreshToken;

        // Act
        var result = await client.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNullRefreshToken_ShouldThrowArgumentException()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var httpClient = new HttpClient();
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.RefreshTokenAsync(null!));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithEmptyRefreshToken_ShouldThrowArgumentException()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var httpClient = new HttpClient();
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.RefreshTokenAsync(string.Empty));
    }

    [Fact]
    public async Task RefreshTokenAsync_UpdatesCache()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        await client.RefreshTokenAsync(BanglalinkAuthClientFixture.TestRefreshToken);
        var cachedToken = client.GetCachedTokenResponse();

        // Assert
        cachedToken.Should().NotBeNull();
    }

    #endregion

    #region Token Caching Tests

    [Fact]
    public async Task GetValidAccessTokenAsync_ReturnsCachedToken_WhenTokenIsValid()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act - First call authenticates
        var token1 = await client.GetValidAccessTokenAsync();
        var token2 = await client.GetValidAccessTokenAsync();

        // Assert - Both should be the same (cached)
        token1.Should().Be(token2);
    }

    [Fact]
    public void GetCachedTokenResponse_ReturnsNull_WhenNoCachedToken()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var httpClient = new HttpClient();
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var cachedToken = client.GetCachedTokenResponse();

        // Assert
        cachedToken.Should().BeNull();
    }

    [Fact]
    public async Task GetCachedTokenResponse_ReturnsToken_WhenTokenIsCached()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        await client.AuthenticateAsync();
        var cachedToken = client.GetCachedTokenResponse();

        // Assert
        cachedToken.Should().NotBeNull();
        cachedToken!.AccessToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Cache Clear Tests

    [Fact]
    public async Task ClearCache_RemovesCachedToken()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        await client.AuthenticateAsync();
        client.ClearCache();
        var cachedToken = client.GetCachedTokenResponse();

        // Assert
        cachedToken.Should().BeNull();
    }

    #endregion

    #region Get Valid Token Response Tests

    [Fact]
    public async Task GetValidTokenResponseAsync_ReturnsTokenResponse()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var result = await client.GetValidTokenResponseAsync();

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetValidTokenResponseAsync_ReturnsCachedResponse_WhenValid()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateValidTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act
        var response1 = await client.GetValidTokenResponseAsync();
        var response2 = await client.GetValidTokenResponseAsync();

        // Assert
        response1.AccessToken.Should().Be(response2.AccessToken);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task AuthenticateAsync_WithServerError_ShouldThrowBanglalinkAuthenticationException()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateErrorTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent, HttpStatusCode.InternalServerError);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<BanglalinkAuthenticationException>(() => client.AuthenticateAsync());
    }

    [Fact]
    public async Task AuthenticateAsync_WithBadRequest_ShouldThrowBanglalinkAuthenticationException()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();
        var responseContent = _fixture.CreateErrorTokenResponse();
        var httpClient = _fixture.CreateHttpClient(responseContent, HttpStatusCode.BadRequest);
        var client = new BanglalinkAuthClient(httpClient, config);

        // Act & Assert
        await Assert.ThrowsAsync<BanglalinkAuthenticationException>(() => client.AuthenticateAsync());
    }

    #endregion
}
