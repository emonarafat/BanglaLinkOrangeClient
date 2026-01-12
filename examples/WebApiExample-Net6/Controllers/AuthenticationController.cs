using Microsoft.AspNetCore.Mvc;
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Exceptions;

namespace WebApiExample.Controllers;

/// <summary>
/// Example controller demonstrating Banglalink OAuth 2.0 client usage in a Web API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IBanglalinkAuthClient _authClient;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IBanglalinkAuthClient authClient, ILogger<AuthenticationController> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets a valid access token.
    /// </summary>
    [HttpGet("token")]
    [Produces("application/json")]
    public async Task<IActionResult> GetTokenAsync()
    {
        try
        {
            _logger.LogInformation("Getting access token");
            var token = await _authClient.GetValidAccessTokenAsync();

            return Ok(new TokenResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                Message = "Token retrieved successfully"
            });
        }
        catch (BanglalinkAuthenticationException ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, new ErrorResponse { Error = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Gets the complete token response with all details.
    /// </summary>
    [HttpGet("token-details")]
    [Produces("application/json")]
    public async Task<IActionResult> GetTokenDetailsAsync()
    {
        try
        {
            _logger.LogInformation("Getting detailed token response");
            var tokenResponse = await _authClient.GetValidTokenResponseAsync();

            return Ok(new DetailedTokenResponse
            {
                AccessToken = tokenResponse.AccessToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                RefreshToken = tokenResponse.RefreshToken,
                RefreshExpiresIn = tokenResponse.RefreshExpiresIn,
                ExpiresAt = tokenResponse.ExpiresAt,
                IsAccessTokenValid = tokenResponse.IsAccessTokenValid,
                IsRefreshTokenValid = tokenResponse.IsRefreshTokenValid
            });
        }
        catch (BanglalinkAuthenticationException ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the cached token without making any HTTP requests.
    /// </summary>
    [HttpGet("cached-token")]
    [Produces("application/json")]
    public IActionResult GetCachedToken()
    {
        _logger.LogInformation("Getting cached token");
        var cachedToken = _authClient.GetCachedTokenResponse();

        if (cachedToken == null)
        {
            return NotFound(new ErrorResponse { Error = "No token cached. Please call /api/authentication/token first." });
        }

        return Ok(new CachedTokenResponse
        {
            IsCached = true,
            IsAccessTokenValid = cachedToken.IsAccessTokenValid,
            IsRefreshTokenValid = cachedToken.IsRefreshTokenValid,
            ExpiresAt = cachedToken.ExpiresAt,
            TimeRemainingSeconds = (int)(cachedToken.ExpiresAt - DateTime.UtcNow).TotalSeconds
        });
    }

    /// <summary>
    /// Clears the cached token.
    /// </summary>
    [HttpPost("clear-cache")]
    [Produces("application/json")]
    public IActionResult ClearCache()
    {
        _logger.LogInformation("Clearing token cache");
        _authClient.ClearCache();

        return Ok(new SuccessResponse { Message = "Token cache cleared successfully" });
    }

    /// <summary>
    /// Manually refreshes the token using a refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [Produces("application/json")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new ErrorResponse { Error = "RefreshToken is required" });
        }

        try
        {
            _logger.LogInformation("Refreshing token");
            var newTokenResponse = await _authClient.RefreshTokenAsync(request.RefreshToken);

            return Ok(new TokenResponse
            {
                AccessToken = newTokenResponse.AccessToken,
                TokenType = newTokenResponse.TokenType,
                Message = "Token refreshed successfully"
            });
        }
        catch (BanglalinkAuthenticationException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}

#region Request/Response Models

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class DetailedTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public int RefreshExpiresIn { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccessTokenValid { get; set; }
    public bool IsRefreshTokenValid { get; set; }
}

public class CachedTokenResponse
{
    public bool IsCached { get; set; }
    public bool IsAccessTokenValid { get; set; }
    public bool IsRefreshTokenValid { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int TimeRemainingSeconds { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

public class SuccessResponse
{
    public string Message { get; set; } = string.Empty;
}

#endregion
