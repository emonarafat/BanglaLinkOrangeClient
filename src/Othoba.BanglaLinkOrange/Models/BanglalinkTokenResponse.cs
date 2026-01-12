using System.Text.Json.Serialization;

namespace Othoba.BanglaLinkOrange.Models;

/// <summary>
/// Represents the OAuth 2.0 token response from Banglalink authorization server.
/// </summary>
public class BanglalinkTokenResponse
{
    /// <summary>
    /// The access token to be used in API requests.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The type of token (typically "Bearer").
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// The number of seconds until the access token expires (default: 36000).
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The refresh token used to obtain a new access token.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// The number of seconds until the refresh token expires (default: 36000).
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    /// <summary>
    /// Indicates when the access token will expire.
    /// </summary>
    [JsonIgnore]
    public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);

    /// <summary>
    /// Indicates when the refresh token will expire.
    /// </summary>
    [JsonIgnore]
    public DateTime RefreshExpiresAt => DateTime.UtcNow.AddSeconds(RefreshExpiresIn);

    /// <summary>
    /// Checks if the access token is still valid.
    /// </summary>
    public bool IsAccessTokenValid => DateTime.UtcNow < ExpiresAt.AddSeconds(-30); // 30-second buffer

    /// <summary>
    /// Checks if the refresh token is still valid.
    /// </summary>
    public bool IsRefreshTokenValid => DateTime.UtcNow < RefreshExpiresAt.AddSeconds(-30); // 30-second buffer
}
