using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Models;

namespace WebApiExample.Services;

/// <summary>
/// Service for handling Banglalink loyalty operations.
/// Provides business logic for loyalty member profile retrieval and analysis.
/// </summary>
public interface ILoyaltyService
{
    /// <summary>
    /// Gets the member loyalty profile.
    /// </summary>
    Task<MemberLoyaltyProfile> GetMemberProfileAsync(string msisdn, string channel = "LMSMYBLAPP", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the member loyalty profile with enriched information.
    /// </summary>
    Task<EnrichedMemberLoyaltyProfile> GetEnrichedMemberProfileAsync(string msisdn, string channel = "LMSMYBLAPP", CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if member is active in loyalty program.
    /// </summary>
    Task<bool> IsMemberActiveAsync(string msisdn, string channel = "LMSMYBLAPP", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets member tier status information.
    /// </summary>
    Task<MemberTierStatus> GetMemberTierStatusAsync(string msisdn, string channel = "LMSMYBLAPP", CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of ILoyaltyService.
/// Handles communication with Banglalink Loyalty API.
/// </summary>
public class LoyaltyService : ILoyaltyService
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<LoyaltyService> _logger;

    public LoyaltyService(
        ILoyaltyClient loyaltyClient,
        IBanglalinkAuthClient authClient,
        ILogger<LoyaltyService> logger)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the member loyalty profile from Banglalink API.
    /// </summary>
    public async Task<MemberLoyaltyProfile> GetMemberProfileAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(msisdn))
            {
                throw new ArgumentException("MSISDN cannot be null or empty.", nameof(msisdn));
            }

            _logger.LogInformation("Retrieving loyalty profile for MSISDN: {Msisdn}", msisdn);

            // Create request
            var request = new LoyaltyMemberProfileRequest
            {
                Channel = channel,
                Msisdn = msisdn,
                TransactionID = GenerateTransactionId()
            };

            // Get access token
            var accessToken = await _authClient.GetValidAccessTokenAsync(cancellationToken);

            // Call loyalty API
            var apiResponse = await _loyaltyClient.GetMemberProfileAsync(request, accessToken, cancellationToken);

            _logger.LogInformation(
                "Successfully retrieved loyalty profile for MSISDN: {Msisdn}",
                msisdn);

            // Map to service model
            return MapToMemberLoyaltyProfile(apiResponse);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            throw;
        }
        catch (LoyaltyApiException ex)
        {
            _logger.LogError(ex, "Loyalty API error for MSISDN: {Msisdn}", msisdn);
            throw new LoyaltyServiceException($"Failed to retrieve member profile: {ex.Message}", ex);
        }
        catch (BanglalinkAuthenticationException ex)
        {
            _logger.LogError(ex, "Authentication failed");
            throw new LoyaltyServiceException("Authentication failed", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving loyalty profile for MSISDN: {Msisdn}", msisdn);
            throw new LoyaltyServiceException("An unexpected error occurred while retrieving member profile", ex);
        }
    }

    /// <summary>
    /// Gets enriched member profile with additional calculations and analysis.
    /// </summary>
    public async Task<EnrichedMemberLoyaltyProfile> GetEnrichedMemberProfileAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default)
    {
        var profile = await GetMemberProfileAsync(msisdn, channel, cancellationToken);

        return new EnrichedMemberLoyaltyProfile
        {
            Msisdn = profile.Msisdn,
            TransactionID = profile.TransactionID,
            StatusCode = profile.StatusCode,
            StatusMsg = profile.StatusMsg,
            ResponseDateTime = profile.ResponseDateTime,
            LoyaltyProfileInfo = profile.LoyaltyProfileInfo,
            IsActive = !string.IsNullOrEmpty(profile.StatusCode) && profile.StatusCode == "0",
            EnrollmentStatus = GetEnrollmentStatus(profile),
            TierExpiryDaysRemaining = CalculateTierExpiryDays(profile.LoyaltyProfileInfo?.ExpiryDate),
            PointsExpiryDaysRemaining = CalculatePointsExpiryDays(profile.LoyaltyProfileInfo?.ExpiryDate),
            HasExpiringPoints = HasExpiringPointsSoon(profile.LoyaltyProfileInfo?.PointsExpiring),
            TierLevel = profile.LoyaltyProfileInfo?.CurrentTierLevel ?? "UNKNOWN"
        };
    }

    /// <summary>
    /// Checks if a member is active in the loyalty program.
    /// </summary>
    public async Task<bool> IsMemberActiveAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await GetMemberProfileAsync(msisdn, channel, cancellationToken);
            return profile.StatusCode == "0" && profile.LoyaltyProfileInfo != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking member active status for MSISDN: {Msisdn}", msisdn);
            return false;
        }
    }

    /// <summary>
    /// Gets member tier status information.
    /// </summary>
    public async Task<MemberTierStatus> GetMemberTierStatusAsync(
        string msisdn,
        string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await GetMemberProfileAsync(msisdn, channel, cancellationToken);

            return new MemberTierStatus
            {
                Msisdn = profile.Msisdn,
                CurrentTier = profile.LoyaltyProfileInfo?.CurrentTierLevel ?? "UNKNOWN",
                TierExpiryDate = profile.LoyaltyProfileInfo?.ExpiryDate,
                EnrolledDate = profile.LoyaltyProfileInfo?.EnrolledDate,
                IsActive = profile.StatusCode == "0",
                DaysUntilExpiry = CalculateTierExpiryDays(profile.LoyaltyProfileInfo?.ExpiryDate)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving tier status for MSISDN: {Msisdn}", msisdn);
            throw new LoyaltyServiceException("Failed to retrieve member tier status", ex);
        }
    }

    #region Private Methods

    private MemberLoyaltyProfile MapToMemberLoyaltyProfile(LoyaltyMemberProfileResponse apiResponse)
    {
        return new MemberLoyaltyProfile
        {
            Msisdn = apiResponse.Msisdn,
            TransactionID = apiResponse.TransactionID,
            StatusCode = apiResponse.StatusCode,
            StatusMsg = apiResponse.StatusMsg,
            ResponseDateTime = apiResponse.ResponseDateTime,
            LoyaltyProfileInfo = apiResponse.LoyaltyProfileInfo
        };
    }

    private string GenerateTransactionId()
    {
        return $"LMS{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Random.Shared.Next(1000, 9999)}";
    }

    private string GetEnrollmentStatus(MemberLoyaltyProfile profile)
    {
        if (profile.StatusCode != "0")
            return "ERROR";

        if (profile.LoyaltyProfileInfo == null)
            return "INACTIVE";

        var enrolledDate = TryParseDateTime(profile.LoyaltyProfileInfo.EnrolledDate);
        if (enrolledDate == null)
            return "UNKNOWN";

        var daysSinceEnrollment = (DateTime.Now - enrolledDate.Value).Days;
        return daysSinceEnrollment > 0 ? "ACTIVE" : "PENDING";
    }

    private int? CalculateTierExpiryDays(string? expiryDate)
    {
        if (string.IsNullOrEmpty(expiryDate))
            return null;

        var expiry = TryParseDateTime(expiryDate);
        if (expiry == null)
            return null;

        var daysRemaining = (expiry.Value.Date - DateTime.Now.Date).Days;
        return daysRemaining > 0 ? daysRemaining : 0;
    }

    private int? CalculatePointsExpiryDays(string? expiryDate)
    {
        return CalculateTierExpiryDays(expiryDate);
    }

    private bool HasExpiringPointsSoon(string? expiringPoints)
    {
        if (string.IsNullOrEmpty(expiringPoints))
            return false;

        return long.TryParse(expiringPoints, out var points) && points > 0;
    }

    private DateTime? TryParseDateTime(string dateTimeString)
    {
        // Try multiple date formats used by Banglalink API
        var formats = new[]
        {
            "dd-MM-yyyy HH:mm:ss",
            "dd-MM-yyyy",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateTimeString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        return null;
    }

    #endregion
}

#region Service Models

public class MemberLoyaltyProfile
{
    public string Msisdn { get; set; } = string.Empty;
    public string TransactionID { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMsg { get; set; } = string.Empty;
    public string ResponseDateTime { get; set; } = string.Empty;
    public LoyaltyProfileInfo? LoyaltyProfileInfo { get; set; }
}

public class EnrichedMemberLoyaltyProfile : MemberLoyaltyProfile
{
    public bool IsActive { get; set; }
    public string EnrollmentStatus { get; set; } = string.Empty;
    public int? TierExpiryDaysRemaining { get; set; }
    public int? PointsExpiryDaysRemaining { get; set; }
    public bool HasExpiringPoints { get; set; }
    public string TierLevel { get; set; } = string.Empty;
}

public class MemberTierStatus
{
    public string Msisdn { get; set; } = string.Empty;
    public string CurrentTier { get; set; } = string.Empty;
    public string? TierExpiryDate { get; set; }
    public string? EnrolledDate { get; set; }
    public bool IsActive { get; set; }
    public int? DaysUntilExpiry { get; set; }
}

#endregion

#region Custom Exceptions

public class LoyaltyServiceException : Exception
{
    public LoyaltyServiceException(string message) : base(message) { }
    public LoyaltyServiceException(string message, Exception innerException) : base(message, innerException) { }
}

#endregion
