using FluentAssertions;
using Othoba.BanglaLinkOrange.Configuration;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for BanglalinkClientConfiguration class.
/// Tests configuration validation and error messages.
/// </summary>
public class BanglalinkClientConfigurationTests
{
    #region Validation Tests

    [Fact]
    public void IsValid_WithAllRequiredProperties_ShouldReturnTrue()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingBaseUrl_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = null!,
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingClientId_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = null!,
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingClientSecret_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = null!,
            Username = "username",
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingUsername_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = null!,
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingPassword_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = null!
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyBaseUrl_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = string.Empty,
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceClientId_ShouldReturnFalse()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "   ",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public void GetValidationErrors_WithAllMissingProperties_ShouldReturnAllErrors()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration();

        // Act
        var errors = config.GetValidationErrors();

        // Assert
        errors.Should().HaveCount(5);
        errors.Should().Contain("BaseUrl is required");
        errors.Should().Contain("ClientId is required");
        errors.Should().Contain("ClientSecret is required");
        errors.Should().Contain("Username is required");
        errors.Should().Contain("Password is required");
    }

    [Fact]
    public void GetValidationErrors_WithMissingBaseUrlAndPassword_ShouldReturnSpecificErrors()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username"
        };

        // Act
        var errors = config.GetValidationErrors();

        // Assert
        errors.Should().HaveCount(2);
        errors.Should().Contain("BaseUrl is required");
        errors.Should().Contain("Password is required");
    }

    [Fact]
    public void GetValidationErrors_WithValidConfiguration_ShouldReturnEmptyList()
    {
        // Arrange
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        var errors = config.GetValidationErrors();

        // Assert
        errors.Should().BeEmpty();
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void DefaultScope_ShouldBeOpenid()
    {
        // Arrange & Act
        var config = new BanglalinkClientConfiguration();

        // Assert
        config.Scope.Should().Be("openid");
    }

    [Fact]
    public void DefaultHttpClientTimeout_ShouldBe30Seconds()
    {
        // Arrange & Act
        var config = new BanglalinkClientConfiguration();

        // Assert
        config.HttpClientTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void DefaultAutoRefreshToken_ShouldBeTrue()
    {
        // Arrange & Act
        var config = new BanglalinkClientConfiguration();

        // Assert
        config.AutoRefreshToken.Should().BeTrue();
    }

    #endregion
}
