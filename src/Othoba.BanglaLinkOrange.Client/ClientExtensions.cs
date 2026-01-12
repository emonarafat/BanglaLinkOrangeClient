using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange.Clients;

namespace Othoba.BanglaLinkOrange.Client;

/// <summary>
/// Extension methods for simplified client registration and usage.
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Gets a pre-configured client factory for fluent configuration.
    /// </summary>
    public static BanglalinkOAuthClientFactory CreateClientFactory()
    {
        return new BanglalinkOAuthClientFactory();
    }

    /// <summary>
    /// Gets a pre-configured client factory with custom service collection.
    /// </summary>
    public static BanglalinkOAuthClientFactory CreateClientFactory(IServiceCollection services)
    {
        return new BanglalinkOAuthClientFactory(services);
    }

    /// <summary>
    /// Creates a quick client with minimal configuration.
    /// </summary>
    public static IBanglalinkAuthClient CreateQuickClient(
        string baseUrl,
        string clientId,
        string clientSecret,
        string username,
        string password)
    {
        return new BanglalinkOAuthClientFactory()
            .WithBaseUrl(baseUrl)
            .WithClientCredentials(clientId, clientSecret)
            .WithUserCredentials(username, password)
            .Build();
    }
}
