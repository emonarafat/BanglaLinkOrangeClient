using FluentAssertions;
using Othoba.BanglaLinkOrange.Models;
using System;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for BanglalinkTokenResponse class.
/// Tests token expiration validation and helper properties.
/// </summary>
public class BanglalinkTokenResponseTests
{
    #region Token Validity Tests

    [Fact]
    public void IsAccessTokenValid_WithFutureExpiration_ShouldReturnTrue()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "test-token",
            ExpiresIn = 3600 // 1 hour from now
        };

        // Act
        var result = response.IsAccessTokenValid;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccessTokenValid_WithPastExpiration_ShouldReturnFalse()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "test-token",
            ExpiresIn = -3600 // 1 hour ago
        };

        // Act
        var result = response.IsAccessTokenValid;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccessTokenValid_WithinBuffer_ShouldReturnFalse()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "test-token",
            ExpiresIn = 20 // 20 seconds (within 30-second buffer)
        };

        // Act
        var result = response.IsAccessTokenValid;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRefreshTokenValid_WithFutureExpiration_ShouldReturnTrue()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            RefreshToken = "refresh-token",
            RefreshExpiresIn = 7200 // 2 hours from now
        };

        // Act
        var result = response.IsRefreshTokenValid;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRefreshTokenValid_WithPastExpiration_ShouldReturnFalse()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            RefreshToken = "refresh-token",
            RefreshExpiresIn = -7200 // 2 hours ago
        };

        // Act
        var result = response.IsRefreshTokenValid;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Expiration Time Tests

    [Fact]
    public void ExpiresAt_ShouldReturnFutureDateTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "test-token",
            ExpiresIn = 3600
        };

        // Act
        var expiresAt = response.ExpiresAt;

        // Assert
        expiresAt.Should().BeAfter(now);
        expiresAt.Should().BeBefore(now.AddSeconds(3600 + 1)); // Allow 1 second margin
    }

    [Fact]
    public void RefreshExpiresAt_ShouldReturnFutureDateTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var response = new BanglalinkTokenResponse
        {
            RefreshToken = "refresh-token",
            RefreshExpiresIn = 7200
        };

        // Act
        var expiresAt = response.RefreshExpiresAt;

        // Assert
        expiresAt.Should().BeAfter(now);
        expiresAt.Should().BeBefore(now.AddSeconds(7200 + 1)); // Allow 1 second margin
    }

    #endregion

    #region Token Type Tests

    [Fact]
    public void TokenType_DefaultValue_ShouldBeBearer()
    {
        // Arrange & Act
        var response = new BanglalinkTokenResponse();

        // Assert
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public void TokenType_CanBeSet()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            TokenType = "Custom"
        };

        // Act & Assert
        response.TokenType.Should().Be("Custom");
    }

    #endregion

    #region Token Content Tests

    [Fact]
    public void AccessToken_ShouldBeStorable()
    {
        // Arrange
        const string token = "test-access-token-value";
        var response = new BanglalinkTokenResponse
        {
            AccessToken = token
        };

        // Act & Assert
        response.AccessToken.Should().Be(token);
    }

    [Fact]
    public void RefreshToken_ShouldBeStorable()
    {
        // Arrange
        const string token = "test-refresh-token-value";
        var response = new BanglalinkTokenResponse
        {
            RefreshToken = token
        };

        // Act & Assert
        response.RefreshToken.Should().Be(token);
    }

    [Fact]
    public void ExpiresIn_ShouldBeStorable()
    {
        // Arrange
        const int expiresIn = 36000;
        var response = new BanglalinkTokenResponse
        {
            ExpiresIn = expiresIn
        };

        // Act & Assert
        response.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact]
    public void RefreshExpiresIn_ShouldBeStorable()
    {
        // Arrange
        const int refreshExpiresIn = 36000;
        var response = new BanglalinkTokenResponse
        {
            RefreshExpiresIn = refreshExpiresIn
        };

        // Act & Assert
        response.RefreshExpiresIn.Should().Be(refreshExpiresIn);
    }

    #endregion

    #region Token Comparison Tests

    [Fact]
    public void BothTokensValid_ShouldAllowAnyOperation()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresIn = 3600,
            RefreshExpiresIn = 7200
        };

        // Act
        var accessValid = response.IsAccessTokenValid;
        var refreshValid = response.IsRefreshTokenValid;

        // Assert
        accessValid.Should().BeTrue();
        refreshValid.Should().BeTrue();
    }

    [Fact]
    public void AccessTokenExpired_ButRefreshTokenValid_ShouldRefresh()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresIn = 20, // Within buffer (invalid)
            RefreshExpiresIn = 7200 // Valid
        };

        // Act
        var accessValid = response.IsAccessTokenValid;
        var refreshValid = response.IsRefreshTokenValid;

        // Assert
        accessValid.Should().BeFalse();
        refreshValid.Should().BeTrue();
    }

    [Fact]
    public void BothTokensExpired_ShouldRequireReAuthentication()
    {
        // Arrange
        var response = new BanglalinkTokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresIn = -3600, // Expired
            RefreshExpiresIn = -7200 // Expired
        };

        // Act
        var accessValid = response.IsAccessTokenValid;
        var refreshValid = response.IsRefreshTokenValid;

        // Assert
        accessValid.Should().BeFalse();
        refreshValid.Should().BeFalse();
    }

    #endregion
}
