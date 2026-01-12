namespace Othoba.BanglaLinkOrange.Models;

/// <summary>
/// Request model for Loyalty Get Member Profile API.
/// Used to retrieve loyalty member's points and segmentation information.
/// </summary>
public class LoyaltyMemberProfileRequest
{
    /// <summary>
    /// Gets or sets the channel name.
    /// Example: "LMSMYBLAPP"
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer MSISDN (Mobile Station Integrated Services Digital Network).
    /// Format: 880XXXXXXXXXX (14 digits starting with country code 880)
    /// Example: "88014########"
    /// </summary>
    public string Msisdn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction ID for tracking and logging purposes.
    /// Unique identifier for this specific API request.
    /// Example: "LMS34197492"
    /// </summary>
    public string TransactionID { get; set; } = string.Empty;
}
