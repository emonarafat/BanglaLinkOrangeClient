namespace Othoba.BanglaLinkOrange.Models;

/// <summary>
/// Represents the loyalty profile information for a member.
/// Contains points balance, tier level, and enrollment details.
/// </summary>
public class LoyaltyProfileInfo
{
    /// <summary>
    /// Gets or sets the available loyalty points for the member.
    /// Example: "10409080"
    /// </summary>
    public string AvailablePoints { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current tier level of the member.
    /// Example: "SIGNATURE", "GOLD", "SILVER", "BRONZE"
    /// </summary>
    public string CurrentTierLevel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiry date of the loyalty membership.
    /// Format: "DD-MM-YYYY"
    /// Example: "31-12-2024"
    /// </summary>
    public string ExpiryDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the points that will expire.
    /// Example: "10409080"
    /// </summary>
    public string PointsExpiring { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the member was enrolled.
    /// Format: "DD-MM-YYYY HH:mm:ss"
    /// Example: "21-11-2022 10:31:30"
    /// </summary>
    public string EnrolledDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel through which the member was enrolled.
    /// May be empty for some members.
    /// </summary>
    public string EnrolledChannel { get; set; } = string.Empty;
}
