namespace Othoba.BanglaLinkOrange.Models;

/// <summary>
/// Response model for Loyalty Get Member Profile API.
/// Contains the member's loyalty information and transaction status.
/// </summary>
public class LoyaltyMemberProfileResponse
{
    /// <summary>
    /// Gets or sets the customer MSISDN.
    /// Example: "88014########"
    /// </summary>
    public string Msisdn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction ID from the request.
    /// Used for tracking and logging.
    /// Example: "LMS34197492"
    /// </summary>
    public string TransactionID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status code of the API response.
    /// 0 = Success
    /// </summary>
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status message.
    /// Example: "SUCCESS"
    /// </summary>
    public string StatusMsg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time of the response.
    /// Format: "DD-MM-YYYY HH:mm:ss"
    /// Example: "10-07-2023 14:49:19"
    /// </summary>
    public string ResponseDateTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the loyalty profile information.
    /// Contains points balance, tier level, and enrollment details.
    /// </summary>
    public LoyaltyProfileInfo? LoyaltyProfileInfo { get; set; }

    /// <summary>
    /// Gets a value indicating whether the API call was successful.
    /// </summary>
    public bool IsSuccessful => StatusCode == "0";
}
