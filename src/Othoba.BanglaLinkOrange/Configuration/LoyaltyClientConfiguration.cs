namespace Othoba.BanglaLinkOrange.Configuration;

/// <summary>
/// Configuration settings for Loyalty API client.
/// Contains endpoint URL and other loyalty API related settings.
/// </summary>
public class LoyaltyClientConfiguration
{
    /// <summary>
    /// Gets or sets the base URL for the Loyalty API.
    /// Example: "http://localhost:8080"
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Loyalty API endpoint path.
    /// Default: "/openapi-lms/loyalty2/get-member-profile"
    /// </summary>
    public string GetMemberProfileEndpoint { get; set; } = "/openapi-lms/loyalty2/get-member-profile";

    /// <summary>
    /// Gets or sets the request content type.
    /// Default: "application/vnd.banglalink.apihub-v1.0+json"
    /// </summary>
    public string ContentType { get; set; } = "application/vnd.banglalink.apihub-v1.0+json";

    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// Default: 30 seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the full URL for the Get Member Profile endpoint.
    /// </summary>
    public string GetMemberProfileUrl => $"{BaseUrl.TrimEnd('/')}{GetMemberProfileEndpoint}";

    /// <summary>
    /// Validates the configuration to ensure all required settings are present.
    /// </summary>
    /// <returns>True if configuration is valid; otherwise false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(GetMemberProfileEndpoint) &&
               !string.IsNullOrWhiteSpace(ContentType) &&
               RequestTimeoutSeconds > 0;
    }
}
