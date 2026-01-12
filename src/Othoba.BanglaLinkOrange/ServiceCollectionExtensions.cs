using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Handlers;
using Polly;

namespace Othoba.BanglaLinkOrange;

/// <summary>
/// Extension methods for registering Banglalink OAuth 2.0 client in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Banglalink authentication client with the provided configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the BanglalinkClientConfiguration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBanglalinkAuthClient(
        this IServiceCollection services,
        Action<BanglalinkClientConfiguration> configure)
    {
        var configuration = new BanglalinkClientConfiguration();
        configure(configuration);

        services.AddHttpClient<IBanglalinkAuthClient, BanglalinkAuthClient>(client =>
        {
            client.Timeout = configuration.HttpClientTimeout;
        })
        .ConfigureHttpClient(_ => { })
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100)));

        services.AddSingleton(configuration);
        services.AddScoped<IBanglalinkAuthClient>(sp =>
            new BanglalinkAuthClient(sp.GetRequiredService<HttpClient>(), configuration));

        return services;
    }

    /// <summary>
    /// Registers the Banglalink authentication client with the provided configuration instance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The BanglalinkClientConfiguration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBanglalinkAuthClient(
        this IServiceCollection services,
        BanglalinkClientConfiguration configuration)
    {
        services.AddHttpClient<IBanglalinkAuthClient, BanglalinkAuthClient>(client =>
        {
            client.Timeout = configuration.HttpClientTimeout;
        });

        services.AddSingleton(configuration);
        services.AddScoped<IBanglalinkAuthClient>(sp =>
            new BanglalinkAuthClient(sp.GetRequiredService<HttpClient>(), configuration));

        return services;
    }

    /// <summary>
    /// Registers the Loyalty API client with the provided configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the LoyaltyClientConfiguration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoyaltyClient(
        this IServiceCollection services,
        Action<LoyaltyClientConfiguration> configure)
    {
        var configuration = new LoyaltyClientConfiguration();
        configure(configuration);

        services.AddHttpClient<ILoyaltyClient, LoyaltyClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(configuration.RequestTimeoutSeconds);
        })
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100)));

        services.AddSingleton(configuration);

        return services;
    }

    /// <summary>
    /// Registers the Loyalty API client with the provided configuration instance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The LoyaltyClientConfiguration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoyaltyClient(
        this IServiceCollection services,
        LoyaltyClientConfiguration configuration)
    {
        services.AddHttpClient<ILoyaltyClient, LoyaltyClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(configuration.RequestTimeoutSeconds);
        });

        services.AddSingleton(configuration);

        return services;
    }

    /// <summary>
    /// Registers the Loyalty API client with auth client integration.
    /// Uses the provided auth client for automatic token management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="loyaltyConfig">The LoyaltyClientConfiguration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <summary>
    /// Adds the Loyalty client with automatic Bearer token injection via DelegatingHandler.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="loyaltyConfig">The LoyaltyClientConfiguration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoyaltyClientWithAuth(
        this IServiceCollection services,
        LoyaltyClientConfiguration loyaltyConfig)
    {
        // Register the AuthenticationDelegatingHandler
        services.AddTransient<AuthenticationDelegatingHandler>();

        // Add HttpClient with delegating handler for automatic token injection
        services.AddHttpClient<ILoyaltyClient, LoyaltyClient>()
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(loyaltyConfig.RequestTimeoutSeconds);
            })
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100)));

        services.AddSingleton(loyaltyConfig);

        return services;
    }
}
