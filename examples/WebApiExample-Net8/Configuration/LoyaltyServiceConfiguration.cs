using Microsoft.Extensions.DependencyInjection;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Configuration;
using WebApiExample.Services;

namespace WebApiExample.Configuration;

/// <summary>
/// Extension methods for registering Banglalink Loyalty API services in the dependency injection container.
/// </summary>
public static class LoyaltyServiceCollectionExtensions
{
    /// <summary>
    /// Adds Banglalink Loyalty API client and related services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="loyaltyConfig">Loyalty API configuration</param>
    /// <returns>The service collection for method chaining</returns>
    /// <example>
    /// <code>
    /// var loyaltyConfig = new LoyaltyApiConfig
    /// {
    ///     BaseUrl = "https://openapi.banglalink.net/",
    ///     Timeout = TimeSpan.FromSeconds(30),
    ///     RetryCount = 3,
    ///     RetryDelay = TimeSpan.FromMilliseconds(500)
    /// };
    ///
    /// services.AddBanglalinkLoyaltyClient(loyaltyConfig);
    /// </code>
    /// </example>
    public static IServiceCollection AddBanglalinkLoyaltyClient(
        this IServiceCollection services,
        LoyaltyApiConfig loyaltyConfig)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loyaltyConfig);

        // Register configuration
        services.AddSingleton(loyaltyConfig);

        // Register Loyalty Client
        services.AddScoped<ILoyaltyClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<LoyaltyClient>>();
            return new LoyaltyClient(
                loyaltyConfig.BaseUrl,
                logger,
                loyaltyConfig.Timeout,
                loyaltyConfig.RetryCount,
                loyaltyConfig.RetryDelay);
        });

        // Register Loyalty Service
        services.AddScoped<ILoyaltyService, LoyaltyService>();

        return services;
    }

    /// <summary>
    /// Adds Banglalink Loyalty API client with configuration loaded from IConfiguration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    /// <example>
    /// <code>
    /// services.AddBanglalinkLoyaltyClient(configuration);
    ///
    /// // appsettings.json
    /// {
    ///   "LoyaltyApi": {
    ///     "BaseUrl": "https://openapi.banglalink.net/",
    ///     "Timeout": "00:00:30",
    ///     "RetryCount": 3,
    ///     "RetryDelay": "00:00:00.5000000"
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddBanglalinkLoyaltyClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var loyaltyConfig = new LoyaltyApiConfig();
        configuration.GetSection("LoyaltyApi").Bind(loyaltyConfig);

        return services.AddBanglalinkLoyaltyClient(loyaltyConfig);
    }
}

/// <summary>
/// Configuration for the Banglalink Loyalty API.
/// </summary>
public class LoyaltyApiConfig
{
    /// <summary>
    /// Gets or sets the base URL for the Loyalty API.
    /// Default: "https://openapi.banglalink.net/"
    /// </summary>
    public string BaseUrl { get; set; } = "https://openapi.banglalink.net/";

    /// <summary>
    /// Gets or sets the HTTP request timeout.
    /// Default: 30 seconds
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the number of retry attempts for failed requests.
    /// Default: 3
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// Default: 500 milliseconds
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
}

/// <summary>
/// Startup configuration for Web API example application.
/// Shows how to configure Banglalink services in an ASP.NET Core application.
/// </summary>
public static class WebApiStartupConfiguration
{
    /// <summary>
    /// Configures Banglalink-related services for the application.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    public static void ConfigureBanglalinkServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add authentication services
        var authConfig = configuration.GetSection("BanglalinkAuth").Get<BanglalinkAuthConfig>()
            ?? throw new InvalidOperationException("BanglalinkAuth configuration is missing");

        services.AddBanglalinkAuthClient(authConfig);

        // Add loyalty services
        services.AddBanglalinkLoyaltyClient(configuration);

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.AddEventSourceLogger();
        });
    }

    /// <summary>
    /// Configures Banglalink authentication client.
    /// </summary>
    private static void AddBanglalinkAuthClient(
        this IServiceCollection services,
        BanglalinkAuthConfig config)
    {
        services.AddScoped<IBanglalinkAuthClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<BanglalinkAuthClient2>>();
            return new BanglalinkAuthClient2(
                config.OAuthBaseUrl,
                config.ClientId,
                config.ClientSecret,
                logger);
        });
    }
}

/// <summary>
/// Configuration for Banglalink Authentication.
/// </summary>
public class BanglalinkAuthConfig
{
    /// <summary>
    /// Gets or sets the OAuth base URL.
    /// </summary>
    public string OAuthBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Unified configuration for Banglalink Auth and Loyalty API.
/// Combines both OAuth and Loyalty API settings in a single configuration object.
/// </summary>
public class BanglalinkConfig
{
    /// <summary>
    /// Gets or sets the OAuth configuration.
    /// </summary>
    public OAuthSettings OAuth { get; set; } = new();

    /// <summary>
    /// Gets or sets the Loyalty API configuration.
    /// </summary>
    public LoyaltySettings Loyalty { get; set; } = new();

    /// <summary>
    /// OAuth configuration settings.
    /// </summary>
    public class OAuthSettings
    {
        /// <summary>
        /// Gets or sets the OAuth base URL.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.banglalink.net/oauth2/";

        /// <summary>
        /// Gets or sets the OAuth client ID.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OAuth client secret.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Loyalty API configuration settings.
    /// </summary>
    public class LoyaltySettings
    {
        /// <summary>
        /// Gets or sets the Loyalty API base URL.
        /// </summary>
        public string BaseUrl { get; set; } = "https://openapi.banglalink.net/";

        /// <summary>
        /// Gets or sets the HTTP request timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the number of retry attempts.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay between retry attempts.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}

/// <summary>
/// Extension methods for registering Banglalink services with unified configuration.
/// </summary>
public static class BanglalinkServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Banglalink services (Auth and Loyalty) with a single unified configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    /// <example>
    /// <code>
    /// // In Program.cs
    /// builder.Services.AddBanglalink(builder.Configuration);
    ///
    /// // In appsettings.json
    /// {
    ///   "Banglalink": {
    ///     "OAuth": {
    ///       "BaseUrl": "https://api.banglalink.net/oauth2/",
    ///       "ClientId": "your_id",
    ///       "ClientSecret": "your_secret",
    ///       "Username": "your_username",
    ///       "Password": "your_password"
    ///     },
    ///     "Loyalty": {
    ///       "BaseUrl": "https://openapi.banglalink.net/",
    ///       "Timeout": "00:00:30",
    ///       "RetryCount": 3,
    ///       "RetryDelay": "00:00:00.5"
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddBanglalink(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Load unified configuration
        var banglalinkConfig = new BanglalinkConfig();
        configuration.GetSection("Banglalink").Bind(banglalinkConfig);

        return services.AddBanglalink(banglalinkConfig);
    }

    /// <summary>
    /// Adds all Banglalink services (Auth and Loyalty) with a unified configuration object.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="config">Banglalink unified configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddBanglalink(
        this IServiceCollection services,
        BanglalinkConfig config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        // Register Auth Client
        services.AddScoped<IBanglalinkAuthClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<BanglalinkAuthClient2>>();
            return new BanglalinkAuthClient2(
                config.OAuth.BaseUrl,
                config.OAuth.ClientId,
                config.OAuth.ClientSecret,
                logger);
        });

        // Register Loyalty Client and Service
        var loyaltyConfig = new LoyaltyApiConfig
        {
            BaseUrl = config.Loyalty.BaseUrl,
            Timeout = config.Loyalty.Timeout,
            RetryCount = config.Loyalty.RetryCount,
            RetryDelay = config.Loyalty.RetryDelay
        };
        services.AddBanglalinkLoyaltyClient(loyaltyConfig);

        return services;
    }
}
