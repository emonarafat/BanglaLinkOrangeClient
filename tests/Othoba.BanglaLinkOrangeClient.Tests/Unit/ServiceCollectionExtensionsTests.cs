using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for ServiceCollectionExtensions.
/// Tests dependency injection registration and client resolution.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region AddBanglalinkAuthClient with Configuration Action Tests

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationAction_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddBanglalinkAuthClient(config =>
        {
            config.BaseUrl = "http://localhost:8080";
            config.ClientId = "client-id";
            config.ClientSecret = "client-secret";
            config.Username = "username";
            config.Password = "password";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IBanglalinkAuthClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationAction_ShouldUseSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddBanglalinkAuthClient(config =>
        {
            config.BaseUrl = "http://localhost:8080";
            config.ClientId = "client-id";
            config.ClientSecret = "client-secret";
            config.Username = "username";
            config.Password = "password";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var client1 = provider.GetService<IBanglalinkAuthClient>();
        var client2 = provider.GetService<IBanglalinkAuthClient>();
        client1.Should().BeSameAs(client2);
    }

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationAction_ShouldApplyConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        const string expectedBaseUrl = "http://custom-url:9090";

        // Act
        services.AddBanglalinkAuthClient(config =>
        {
            config.BaseUrl = expectedBaseUrl;
            config.ClientId = "client-id";
            config.ClientSecret = "client-secret";
            config.Username = "username";
            config.Password = "password";
            config.Scope = "custom-scope";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var config = provider.GetService<BanglalinkClientConfiguration>();
        config.Should().NotBeNull();
        config!.BaseUrl.Should().Be(expectedBaseUrl);
        config.Scope.Should().Be("custom-scope");
    }

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationAction_WithInvalidConfig_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act & Assert
        var action = () => services.AddBanglalinkAuthClient(config =>
        {
            // Not setting required properties
        });

        action.Should().Throw<Exception>();
    }

    #endregion

    #region AddBanglalinkAuthClient with Configuration Instance Tests

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationInstance_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        services.AddBanglalinkAuthClient(config);

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IBanglalinkAuthClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationInstance_ShouldUseSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://localhost:8080",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Username = "username",
            Password = "password"
        };

        // Act
        services.AddBanglalinkAuthClient(config);

        // Assert
        var provider = services.BuildServiceProvider();
        var client1 = provider.GetService<IBanglalinkAuthClient>();
        var client2 = provider.GetService<IBanglalinkAuthClient>();
        client1.Should().BeSameAs(client2);
    }

    [Fact]
    public void AddBanglalinkAuthClient_WithConfigurationInstance_ShouldUseProvidedConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var config = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://custom:9090",
            ClientId = "my-client",
            ClientSecret = "my-secret",
            Username = "myuser",
            Password = "mypass",
            Scope = "custom-scope"
        };

        // Act
        services.AddBanglalinkAuthClient(config);

        // Assert
        var provider = services.BuildServiceProvider();
        var registeredConfig = provider.GetService<BanglalinkClientConfiguration>();
        registeredConfig.Should().NotBeNull();
        registeredConfig!.BaseUrl.Should().Be(config.BaseUrl);
        registeredConfig.Scope.Should().Be(config.Scope);
    }

    [Fact]
    public void AddBanglalinkAuthClient_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        BanglalinkClientConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddBanglalinkAuthClient(config!));
    }

    #endregion

    #region HttpClient Registration Tests

    [Fact]
    public void AddBanglalinkAuthClient_ShouldRegisterHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddBanglalinkAuthClient(config =>
        {
            config.BaseUrl = "http://localhost:8080";
            config.ClientId = "client-id";
            config.ClientSecret = "client-secret";
            config.Username = "username";
            config.Password = "password";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var httpClient = provider.GetService<HttpClient>();
        httpClient.Should().NotBeNull();
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void AddBanglalinkAuthClient_WithMissingRequiredField_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act & Assert
        var action = () => services.AddBanglalinkAuthClient(config =>
        {
            config.BaseUrl = "http://localhost:8080";
            config.ClientId = "client-id";
            // Missing other required fields
        });

        action.Should().Throw<Exception>();
    }

    #endregion

    #region Multiple Registrations Tests

    [Fact]
    public void AddBanglalinkAuthClient_CalledTwice_SecondRegistrationShouldOverride()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var firstConfig = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://first:8080",
            ClientId = "first-id",
            ClientSecret = "first-secret",
            Username = "first-user",
            Password = "first-pass"
        };
        var secondConfig = new BanglalinkClientConfiguration
        {
            BaseUrl = "http://second:9090",
            ClientId = "second-id",
            ClientSecret = "second-secret",
            Username = "second-user",
            Password = "second-pass"
        };

        // Act
        services.AddBanglalinkAuthClient(firstConfig);
        services.AddBanglalinkAuthClient(secondConfig);

        // Assert
        var provider = services.BuildServiceProvider();
        var config = provider.GetService<BanglalinkClientConfiguration>();
        config!.BaseUrl.Should().Be(secondConfig.BaseUrl);
        config.ClientId.Should().Be(secondConfig.ClientId);
    }

    #endregion
}
