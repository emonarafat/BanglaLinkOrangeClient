namespace Othoba.BanglaLinkOrange.Configuration;

/// <summary>
/// Configuration settings for the Banglalink OAuth 2.0 client.
/// </summary>
public class BanglalinkClientConfiguration
{
    /// <summary>
    /// The base URL of the authorization server (e.g., http://1.2.3.4:8080).
    /// Test and Production URLs will be provided by Banglalink.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The client ID provided by Banglalink.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret provided by Banglalink.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The username for authentication (provided by Banglalink).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for authentication (provided by Banglalink).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth scope (default: "openid").
    /// </summary>
    public string Scope { get; set; } = "openid";

    /// <summary>
    /// Timeout for HTTP requests (default: 30 seconds).
    /// </summary>
    public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to automatically refresh tokens when they expire.
    /// </summary>
    public bool AutoRefreshToken { get; set; } = true;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl)
            && !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(ClientSecret)
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password);
    }

    /// <summary>
    /// Gets validation error messages.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(BaseUrl)) errors.Add("BaseUrl is required");
        if (string.IsNullOrWhiteSpace(ClientId)) errors.Add("ClientId is required");
        if (string.IsNullOrWhiteSpace(ClientSecret)) errors.Add("ClientSecret is required");
        if (string.IsNullOrWhiteSpace(Username)) errors.Add("Username is required");
        if (string.IsNullOrWhiteSpace(Password)) errors.Add("Password is required");
        return errors;
    }
}
