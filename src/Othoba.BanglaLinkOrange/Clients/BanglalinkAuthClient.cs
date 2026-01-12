using System.Net.Http.Headers;
using System.Text.Json;
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Models;
using Othoba.BanglaLinkOrange.Utilities;

namespace Othoba.BanglaLinkOrange.Clients;

/// <summary>
/// Client for Banglalink OAuth 2.0 authentication and authorization.
/// Implements the Password Grant and Refresh Token Grant flows as per the Banglalink API specification.
/// </summary>
public interface IBanglalinkAuthClient
{
    /// <summary>
    /// Gets the current valid access token, refreshing if necessary.
    /// </summary>
    /// <returns>A valid access token.</returns>
    Task<string> GetValidAccessTokenAsync();

    /// <summary>
    /// Gets the current valid access token response, refreshing if necessary.
    /// </summary>
    /// <returns>The token response with all token data.</returns>
    Task<BanglalinkTokenResponse> GetValidTokenResponseAsync();

    /// <summary>
    /// Authenticates with the Banglalink server using username and password.
    /// </summary>
    /// <returns>The token response containing access and refresh tokens.</returns>
    Task<BanglalinkTokenResponse> AuthenticateAsync();

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token from a previous authentication.</param>
    /// <returns>The new token response.</returns>
    Task<BanglalinkTokenResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Gets the current cached token response without refreshing.
    /// </summary>
    BanglalinkTokenResponse? GetCachedTokenResponse();

    /// <summary>
    /// Clears the cached token response.
    /// </summary>
    void ClearCache();
}

/// <summary>
/// Implementation of the Banglalink OAuth 2.0 authentication client.
/// </summary>
public class BanglalinkAuthClient : IBanglalinkAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly BanglalinkClientConfiguration _configuration;
    private BanglalinkTokenResponse? _cachedTokenResponse;
    private readonly object _lockObject = new object();
    private readonly string _tokenEndpoint;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the BanglalinkAuthClient class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="configuration">The Banglalink client configuration.</param>
    /// <exception cref="BanglalinkConfigurationException">Thrown when configuration is invalid.</exception>
    public BanglalinkAuthClient(HttpClient httpClient, BanglalinkClientConfiguration configuration)
    {
        if (!configuration.IsValid())
        {
            var errors = string.Join(", ", configuration.GetValidationErrors());
            throw new BanglalinkConfigurationException($"Invalid configuration: {errors}");
        }

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient.Timeout = _configuration.HttpClientTimeout;

        // Construct the token endpoint as per API specification
        _tokenEndpoint = $"{configuration.BaseUrl.TrimEnd('/')}/auth/realms/banglalink/protocol/openid-connect/token";
    }

    /// <summary>
    /// Gets a valid access token, automatically refreshing if necessary.
    /// </summary>
    public async Task<string> GetValidAccessTokenAsync()
    {
        var response = await GetValidTokenResponseAsync();
        return response.AccessToken;
    }

    /// <summary>
    /// Gets a valid token response, automatically refreshing if necessary.
    /// </summary>
    public async Task<BanglalinkTokenResponse> GetValidTokenResponseAsync()
    {
        lock (_lockObject)
        {
            if (_cachedTokenResponse?.IsAccessTokenValid == true)
            {
                return _cachedTokenResponse;
            }
        }

        // If auto-refresh is enabled and refresh token is valid, refresh
        if (_configuration.AutoRefreshToken && _cachedTokenResponse?.IsRefreshTokenValid == true)
        {
            return await RefreshTokenAsync(_cachedTokenResponse.RefreshToken);
        }

        // Otherwise, authenticate fresh
        return await AuthenticateAsync();
    }

    /// <summary>
    /// Authenticates using the Password Grant flow (username and password).
    /// </summary>
    public async Task<BanglalinkTokenResponse> AuthenticateAsync()
    {
        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", _configuration.Username },
            { "password", _configuration.Password },
            { "client_id", _configuration.ClientId },
            { "scope", _configuration.Scope }
        };

        var response = await SendTokenRequestAsync(requestBody);
        
        lock (_lockObject)
        {
            _cachedTokenResponse = response;
        }

        return response;
    }

    /// <summary>
    /// Refreshes the access token using the Refresh Token Grant flow.
    /// </summary>
    public async Task<BanglalinkTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", _configuration.ClientId }
        };

        var response = await SendTokenRequestAsync(requestBody);

        lock (_lockObject)
        {
            _cachedTokenResponse = response;
        }

        return response;
    }

    /// <summary>
    /// Gets the currently cached token response without making any requests.
    /// </summary>
    public BanglalinkTokenResponse? GetCachedTokenResponse()
    {
        lock (_lockObject)
        {
            return _cachedTokenResponse;
        }
    }

    /// <summary>
    /// Clears the cached token response.
    /// </summary>
    public void ClearCache()
    {
        lock (_lockObject)
        {
            _cachedTokenResponse = null;
        }
    }

    private async Task<BanglalinkTokenResponse> SendTokenRequestAsync(Dictionary<string, string> body)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(body)
            };

            // Add Basic Authentication header
            var basicAuthToken = BasicAuthenticationGenerator.GenerateToken(_configuration.ClientId, _configuration.ClientSecret);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new BanglalinkAuthenticationException(
                    $"Authentication failed with status {response.StatusCode}: {errorContent}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<BanglalinkTokenResponse>(jsonContent, JsonOptions);

            if (tokenResponse == null)
            {
                throw new BanglalinkAuthenticationException("Failed to deserialize token response.");
            }

            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new BanglalinkAuthenticationException("HTTP request failed while attempting authentication.", ex);
        }
        catch (JsonException ex)
        {
            throw new BanglalinkAuthenticationException("Failed to parse authentication response.", ex);
        }
    }
}
