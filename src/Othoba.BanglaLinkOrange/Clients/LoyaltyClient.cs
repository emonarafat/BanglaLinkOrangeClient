using System.Text.Json;
using Othoba.BanglaLinkOrange.Configuration;
using Othoba.BanglaLinkOrange.Exceptions;
using Othoba.BanglaLinkOrange.Models;

namespace Othoba.BanglaLinkOrange.Clients;

/// <summary>
/// Client for Loyalty API operations.
/// Handles member profile retrieval and loyalty information management.
/// Token is automatically managed through the AuthenticationDelegatingHandler.
/// </summary>
public class LoyaltyClient : ILoyaltyClient
{
    private readonly HttpClient _httpClient;
    private readonly LoyaltyClientConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for API requests. Token is managed by AuthenticationDelegatingHandler.</param>
    /// <param name="configuration">The loyalty API configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or configuration is null.</exception>
    public LoyaltyClient(
        HttpClient httpClient,
        LoyaltyClientConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Set default timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.RequestTimeoutSeconds);
    }

    /// <summary>
    /// Retrieves the loyalty member profile information.
    /// The access token is automatically injected by the AuthenticationDelegatingHandler.
    /// </summary>
    public async Task<LoyaltyMemberProfileResponse> GetMemberProfileAsync(
        LoyaltyMemberProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateRequest(request);

        return await ExecuteGetMemberProfileAsync(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves the loyalty member profile information with explicit access token.
    /// </summary>
    [Obsolete("Use GetMemberProfileAsync(request, cancellationToken) instead. Token is now managed automatically via AuthenticationDelegatingHandler.")]
    public async Task<LoyaltyMemberProfileResponse> GetMemberProfileAsync(
        LoyaltyMemberProfileRequest request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

        ValidateRequest(request);

        return await ExecuteGetMemberProfileAsync(request, accessToken, cancellationToken);
    }

    /// <summary>
    /// Executes the member profile API request.
    /// Token is automatically added by AuthenticationDelegatingHandler.
    /// </summary>
    private async Task<LoyaltyMemberProfileResponse> ExecuteGetMemberProfileAsync(
        LoyaltyMemberProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var content = new StringContent(requestJson, System.Text.Encoding.UTF8, _configuration.ContentType);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetMemberProfileUrl)
            {
                Content = content
            };

            // Token is automatically added by AuthenticationDelegatingHandler
            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateException(response.StatusCode, responseContent, request.TransactionID);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var memberResponse = JsonSerializer.Deserialize<LoyaltyMemberProfileResponse>(responseContent, options);

            if (memberResponse == null)
            {
                throw new LoyaltyApiException(
                    "Failed to deserialize member profile response.",
                    (int)response.StatusCode);
            }

            // Check API-level success indicator
            if (!memberResponse.IsSuccessful)
            {
                throw new LoyaltyApiException(
                    $"API returned failure status: {memberResponse.StatusMsg}",
                    (int)response.StatusCode)
                {
                    TransactionID = memberResponse.TransactionID,
                    ErrorCode = memberResponse.StatusCode
                };
            }

            return memberResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new LoyaltyApiException(
                $"HTTP request failed while retrieving member profile: {ex.Message}",
                ex);
        }
        catch (JsonException ex)
        {
            throw new LoyaltyApiException(
                $"Failed to parse API response: {ex.Message}",
                ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new LoyaltyApiException(
                "Request was cancelled.",
                ex);
        }
        catch (LoyaltyApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LoyaltyApiException(
                $"An unexpected error occurred: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Executes the member profile API request with explicit access token.
    /// </summary>
    [Obsolete("Use ExecuteGetMemberProfileAsync(request, cancellationToken) instead. Token is now managed automatically.")]
    private async Task<LoyaltyMemberProfileResponse> ExecuteGetMemberProfileAsync(
        LoyaltyMemberProfileRequest request,
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var content = new StringContent(requestJson, System.Text.Encoding.UTF8, _configuration.ContentType);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetMemberProfileUrl)
            {
                Content = content
            };

            requestMessage.Headers.Add("authorization", $"Bearer {accessToken}");

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateException(response.StatusCode, responseContent, request.TransactionID);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var memberResponse = JsonSerializer.Deserialize<LoyaltyMemberProfileResponse>(responseContent, options);

            if (memberResponse == null)
            {
                throw new LoyaltyApiException(
                    "Failed to deserialize member profile response.",
                    (int)response.StatusCode);
            }

            // Check API-level success indicator
            if (!memberResponse.IsSuccessful)
            {
                throw new LoyaltyApiException(
                    $"API returned failure status: {memberResponse.StatusMsg}",
                    (int)response.StatusCode)
                {
                    TransactionID = memberResponse.TransactionID,
                    ErrorCode = memberResponse.StatusCode
                };
            }

            return memberResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new LoyaltyApiException(
                $"HTTP request failed while retrieving member profile: {ex.Message}",
                ex);
        }
        catch (JsonException ex)
        {
            throw new LoyaltyApiException(
                $"Failed to parse API response: {ex.Message}",
                ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new LoyaltyApiException(
                "Request was cancelled.",
                ex);
        }
        catch (LoyaltyApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LoyaltyApiException(
                $"An unexpected error occurred: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Validates the member profile request.
    /// </summary>
    private void ValidateRequest(LoyaltyMemberProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Channel))
            throw new ArgumentException("Channel cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Msisdn))
            throw new ArgumentException("MSISDN cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.TransactionID))
            throw new ArgumentException("TransactionID cannot be null or empty.", nameof(request));
    }

    /// <summary>
    /// Creates an appropriate exception based on HTTP status code.
    /// </summary>
    private LoyaltyApiException CreateException(
        System.Net.HttpStatusCode statusCode,
        string responseContent,
        string transactionId)
    {
        var message = statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Unauthorized: Full authentication is required to access this resource.",
            System.Net.HttpStatusCode.Forbidden => "Forbidden: Access is denied.",
            System.Net.HttpStatusCode.BadRequest => "Bad Request: Invalid request parameters.",
            System.Net.HttpStatusCode.NotFound => "Not Found: The requested resource was not found.",
            System.Net.HttpStatusCode.InternalServerError => "Internal Server Error: The API server encountered an error.",
            _ => $"API request failed with status code: {statusCode}"
        };

        return new LoyaltyApiException(message, (int)statusCode)
        {
            TransactionID = transactionId,
            ErrorCode = ((int)statusCode).ToString()
        };
    }
}
