using System.Text;

namespace Othoba.BanglaLinkOrange.Utilities;

/// <summary>
/// Utility class for generating Basic Authentication tokens.
/// </summary>
public static class BasicAuthenticationGenerator
{
    /// <summary>
    /// Generates a Basic Authentication token from client ID and secret.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <returns>The Base64-encoded Basic auth token.</returns>
    /// <remarks>
    /// Basic Auth format: base64(client_id:client_secret)
    /// Reference: https://mixedanalytics.com/tools/basic-authentication-generator/
    /// </remarks>
    public static string GenerateToken(string clientId, string clientSecret)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId));

        if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentNullException(nameof(clientSecret));

        var credentials = $"{clientId}:{clientSecret}";
        var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        return Convert.ToBase64String(credentialsBytes);
    }
}
