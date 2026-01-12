using Othoba.BanglaLinkOrange.Models;

namespace Othoba.BanglaLinkOrange.Clients;

/// <summary>
/// Interface for Loyalty API client operations.
/// Defines contracts for retrieving member profile and loyalty information.
/// Access token is automatically managed via AuthenticationDelegatingHandler.
/// </summary>
public interface ILoyaltyClient
{
    /// <summary>
    /// Retrieves the loyalty member profile information.
    /// The access token is automatically injected by the AuthenticationDelegatingHandler.
    /// </summary>
    /// <param name="request">The member profile request containing MSISDN and other details.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The member profile response containing loyalty information.</returns>
    /// <exception cref="LoyaltyApiException">Thrown when the API call fails.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    Task<LoyaltyMemberProfileResponse> GetMemberProfileAsync(
        LoyaltyMemberProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the loyalty member profile information with explicit access token.
    /// </summary>
    /// <param name="request">The member profile request containing MSISDN and other details.</param>
    /// <param name="accessToken">The OAuth access token for authentication.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The member profile response containing loyalty information.</returns>
    /// <exception cref="LoyaltyApiException">Thrown when the API call fails.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    [Obsolete("Use GetMemberProfileAsync(request, cancellationToken) instead. Token is now managed automatically via AuthenticationDelegatingHandler.")]
    Task<LoyaltyMemberProfileResponse> GetMemberProfileAsync(
        LoyaltyMemberProfileRequest request,
        string accessToken,
        CancellationToken cancellationToken = default);
}
