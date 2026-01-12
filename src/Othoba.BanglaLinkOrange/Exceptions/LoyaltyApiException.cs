namespace Othoba.BanglaLinkOrange.Exceptions;

/// <summary>
/// Exception thrown when Loyalty API operations fail.
/// Includes HTTP status codes and detailed error information.
/// </summary>
public class LoyaltyApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LoyaltyApiException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LoyaltyApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code from the API response.</param>
    public LoyaltyApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code from the API response.</param>
    /// <param name="innerException">The inner exception.</param>
    public LoyaltyApiException(string message, int statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code from the API response, if available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets or sets the API error code, if provided by the service.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the transaction ID associated with the failed request.
    /// Useful for troubleshooting and support.
    /// </summary>
    public string? TransactionID { get; set; }
}
