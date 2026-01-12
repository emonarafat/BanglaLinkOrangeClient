using Microsoft.AspNetCore.Mvc;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Models;

namespace WebApiExample.Controllers;

/// <summary>
/// Example controller demonstrating Banglalink Loyalty API client usage in a Web API.
/// Provides endpoints for retrieving member loyalty information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<LoyaltyController> _logger;

    public LoyaltyController(
        ILoyaltyClient loyaltyClient,
        IBanglalinkAuthClient authClient,
        ILogger<LoyaltyController> logger)
    {
        _loyaltyClient = loyaltyClient;
        _authClient = authClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the member loyalty profile information.
    /// </summary>
    /// <param name="msisdn">Customer MSISDN (Mobile Station Integrated Services Digital Network)</param>
    /// <param name="channel">Channel name (e.g., LMSMYBLAPP)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The member's loyalty profile information</returns>
    /// <response code="200">Member profile retrieved successfully</response>
    /// <response code="400">Bad request - missing or invalid parameters</response>
    /// <response code="401">Unauthorized - authentication failed</response>
    /// <response code="403">Forbidden - access denied</response>
    /// <response code="404">Member not found</response>
    [HttpGet("member-profile")]
    [Produces("application/json")]
    public async Task<IActionResult> GetMemberProfileAsync(
        [FromQuery(Name = "msisdn")] string msisdn,
        [FromQuery(Name = "channel")] string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting loyalty profile for MSISDN: {Msisdn}", msisdn);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(msisdn))
            {
                return BadRequest(new ErrorResponse { Error = "MSISDN is required" });
            }

            // Create request
            var request = new LoyaltyMemberProfileRequest
            {
                Channel = channel,
                Msisdn = msisdn,
                TransactionID = Guid.NewGuid().ToString()
            };

            // Get access token
            var accessToken = await _authClient.GetValidAccessTokenAsync(cancellationToken);

            // Call loyalty API
            var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken, cancellationToken);

            _logger.LogInformation(
                "Loyalty profile retrieved successfully for MSISDN: {Msisdn}",
                msisdn);

            return Ok(new MemberProfileResponse
            {
                Msisdn = response.Msisdn,
                TransactionID = response.TransactionID,
                StatusCode = response.StatusCode,
                StatusMsg = response.StatusMsg,
                ResponseDateTime = response.ResponseDateTime,
                LoyaltyProfileInfo = response.LoyaltyProfileInfo,
                Message = "Member profile retrieved successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (LoyaltyApiException ex)
        {
            _logger.LogError(ex, "Loyalty API error");
            return StatusCode((int?)ex.StatusCode ?? 500, new ErrorResponse
            {
                Error = ex.Message,
                ErrorCode = ex.ErrorCode,
                TransactionID = ex.TransactionID
            });
        }
        catch (BanglalinkAuthenticationException ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return Unauthorized(new ErrorResponse { Error = "Authentication failed: " + ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, new ErrorResponse { Error = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Gets member profile with detailed information and tier status.
    /// </summary>
    /// <param name="msisdn">Customer MSISDN</param>
    /// <param name="channel">Channel name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed member profile with tier and points information</returns>
    [HttpGet("member-profile-details")]
    [Produces("application/json")]
    public async Task<IActionResult> GetMemberProfileDetailsAsync(
        [FromQuery(Name = "msisdn")] string msisdn,
        [FromQuery(Name = "channel")] string channel = "LMSMYBLAPP",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting detailed loyalty profile for MSISDN: {Msisdn}", msisdn);

            if (string.IsNullOrWhiteSpace(msisdn))
            {
                return BadRequest(new ErrorResponse { Error = "MSISDN is required" });
            }

            var request = new LoyaltyMemberProfileRequest
            {
                Channel = channel,
                Msisdn = msisdn,
                TransactionID = Guid.NewGuid().ToString()
            };

            var accessToken = await _authClient.GetValidAccessTokenAsync(cancellationToken);
            var response = await _loyaltyClient.GetMemberProfileAsync(request, accessToken, cancellationToken);

            if (response.LoyaltyProfileInfo == null)
            {
                return NotFound(new ErrorResponse { Error = "Member profile information not found" });
            }

            _logger.LogInformation("Detailed profile retrieved for MSISDN: {Msisdn}", msisdn);

            return Ok(new DetailedMemberProfileResponse
            {
                Msisdn = response.Msisdn,
                TransactionID = response.TransactionID,
                StatusCode = response.StatusCode,
                StatusMsg = response.StatusMsg,
                ResponseDateTime = response.ResponseDateTime,
                Tier = new TierInformation
                {
                    CurrentLevel = response.LoyaltyProfileInfo.CurrentTierLevel,
                    ExpiryDate = response.LoyaltyProfileInfo.ExpiryDate
                },
                Points = new PointsInformation
                {
                    AvailablePoints = response.LoyaltyProfileInfo.AvailablePoints,
                    ExpiringPoints = response.LoyaltyProfileInfo.PointsExpiring
                },
                Enrollment = new EnrollmentInformation
                {
                    EnrolledDate = response.LoyaltyProfileInfo.EnrolledDate,
                    EnrolledChannel = response.LoyaltyProfileInfo.EnrolledChannel
                },
                Message = "Detailed member profile retrieved successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (LoyaltyApiException ex)
        {
            _logger.LogError(ex, "Loyalty API error");
            return StatusCode((int?)ex.StatusCode ?? 500, new ErrorResponse
            {
                Error = ex.Message,
                ErrorCode = ex.ErrorCode,
                TransactionID = ex.TransactionID
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, new ErrorResponse { Error = "An unexpected error occurred" });
        }
    }
}

#region Request/Response Models

public class MemberProfileResponse
{
    public string Msisdn { get; set; } = string.Empty;
    public string TransactionID { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMsg { get; set; } = string.Empty;
    public string ResponseDateTime { get; set; } = string.Empty;
    public LoyaltyProfileInfo? LoyaltyProfileInfo { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DetailedMemberProfileResponse
{
    public string Msisdn { get; set; } = string.Empty;
    public string TransactionID { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMsg { get; set; } = string.Empty;
    public string ResponseDateTime { get; set; } = string.Empty;
    public TierInformation? Tier { get; set; }
    public PointsInformation? Points { get; set; }
    public EnrollmentInformation? Enrollment { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TierInformation
{
    public string CurrentLevel { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
}

public class PointsInformation
{
    public string AvailablePoints { get; set; } = string.Empty;
    public string ExpiringPoints { get; set; } = string.Empty;
}

public class EnrollmentInformation
{
    public string EnrolledDate { get; set; } = string.Empty;
    public string EnrolledChannel { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? TransactionID { get; set; }
}

public class SuccessResponse
{
    public string Message { get; set; } = string.Empty;
}

#endregion
