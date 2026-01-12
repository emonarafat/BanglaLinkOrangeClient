namespace Othoba.BanglaLinkOrange.Exceptions;

/// <summary>
/// Base exception for Banglalink client operations.
/// </summary>
public class BanglalinkClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the BanglalinkClientException class.
    /// </summary>
    public BanglalinkClientException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the BanglalinkClientException class with an inner exception.
    /// </summary>
    public BanglalinkClientException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when authentication fails.
/// </summary>
public class BanglalinkAuthenticationException : BanglalinkClientException
{
    /// <summary>
    /// Initializes a new instance of the BanglalinkAuthenticationException class.
    /// </summary>
    public BanglalinkAuthenticationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the BanglalinkAuthenticationException class with an inner exception.
    /// </summary>
    public BanglalinkAuthenticationException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when configuration is invalid.
/// </summary>
public class BanglalinkConfigurationException : BanglalinkClientException
{
    /// <summary>
    /// Initializes a new instance of the BanglalinkConfigurationException class.
    /// </summary>
    public BanglalinkConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the BanglalinkConfigurationException class with an inner exception.
    /// </summary>
    public BanglalinkConfigurationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
