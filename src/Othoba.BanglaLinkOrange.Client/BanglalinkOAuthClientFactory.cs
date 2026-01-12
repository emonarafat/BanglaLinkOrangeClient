using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;

namespace Othoba.BanglaLinkOrange.Client;

/// <summary>
/// Factory class for creating and configuring Banglalink OAuth 2.0 client instances.
/// Simplifies client creation with fluent configuration API.
/// </summary>
public class BanglalinkOAuthClientFactory
{
    private readonly BanglalinkClientConfiguration _configuration;
    private readonly IServiceCollection _services;

    /// <summary>
    /// Creates a new factory instance with default services.
    /// </summary>
    public BanglalinkOAuthClientFactory()
    {
        _services = new ServiceCollection();
        _configuration = new BanglalinkClientConfiguration();
    }

    /// <summary>
    /// Creates a new factory instance with custom services.
    /// </summary>
    public BanglalinkOAuthClientFactory(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = new BanglalinkClientConfiguration();
    }

    /// <summary>
    /// Sets the base URL for the OAuth endpoint.
    /// </summary>
    public BanglalinkOAuthClientFactory WithBaseUrl(string baseUrl)
    {
        _configuration.BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        return this;
    }

    /// <summary>
    /// Sets the client credentials (ID and secret).
    /// </summary>
    public BanglalinkOAuthClientFactory WithClientCredentials(string clientId, string clientSecret)
    {
        _configuration.ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _configuration.ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        return this;
    }

    /// <summary>
    /// Sets the user credentials (username and password).
    /// </summary>
    public BanglalinkOAuthClientFactory WithUserCredentials(string username, string password)
    {
        _configuration.Username = username ?? throw new ArgumentNullException(nameof(username));
        _configuration.Password = password ?? throw new ArgumentNullException(nameof(password));
        return this;
    }

    /// <summary>
    /// Sets the OAuth scope (default: "openid").
    /// </summary>
    public BanglalinkOAuthClientFactory WithScope(string scope)
    {
        _configuration.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        return this;
    }

    /// <summary>
    /// Sets the HTTP client timeout.
    /// </summary>
    public BanglalinkOAuthClientFactory WithHttpClientTimeout(TimeSpan timeout)
    {
        _configuration.HttpClientTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Enables or disables automatic token refresh (default: true).
    /// </summary>
    public BanglalinkOAuthClientFactory WithAutoRefreshToken(bool enabled)
    {
        _configuration.AutoRefreshToken = enabled;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured IBanglalinkAuthClient instance.
    /// </summary>
    public IBanglalinkAuthClient Build()
    {
        // Validate configuration
        var validationErrors = _configuration.GetValidationErrors();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors)}"
            );
        }

        // Register the client with the service collection
        _services.AddBanglalinkAuthClient(_configuration);

        // Build service provider and get the client
        var serviceProvider = _services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IBanglalinkAuthClient>();
    }

    /// <summary>
    /// Builds and returns the configured IBanglalinkAuthClient instance asynchronously.
    /// This method allows for initialization patterns that may require async operations.
    /// </summary>
    public async Task<IBanglalinkAuthClient> BuildAsync()
    {
        return await Task.FromResult(Build());
    }
}
