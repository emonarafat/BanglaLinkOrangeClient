using FluentAssertions;
using Othoba.BanglaLinkOrange.Exceptions;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for BanglalinkClientException and derived exception types.
/// Tests exception hierarchy and error message handling.
/// </summary>
public class ExceptionTests
{
    #region BanglalinkClientException Tests

    [Fact]
    public void BanglalinkClientException_WithMessage_ShouldCreateException()
    {
        // Arrange
        const string message = "An error occurred in Banglalink client";

        // Act
        var exception = new BanglalinkClientException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void BanglalinkClientException_WithMessageAndInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        const string message = "An error occurred";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new BanglalinkClientException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    #endregion

    #region BanglalinkConfigurationException Tests

    [Fact]
    public void BanglalinkConfigurationException_WithMessage_ShouldCreateException()
    {
        // Arrange
        const string message = "Configuration is invalid";

        // Act
        var exception = new BanglalinkConfigurationException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Should().BeOfType<BanglalinkClientException>();
    }

    [Fact]
    public void BanglalinkConfigurationException_InheritsFromBanglalinkClientException()
    {
        // Arrange & Act
        var exception = new BanglalinkConfigurationException("Test");

        // Assert
        exception.Should().BeAssignableTo<BanglalinkClientException>();
    }

    [Fact]
    public void BanglalinkConfigurationException_WithMultipleErrors_ShouldPreserveAllMessages()
    {
        // Arrange
        const string message = "Configuration validation failed: BaseUrl is required, ClientId is required";

        // Act
        var exception = new BanglalinkConfigurationException(message);

        // Assert
        exception.Message.Should().Contain("BaseUrl");
        exception.Message.Should().Contain("ClientId");
    }

    #endregion

    #region BanglalinkAuthenticationException Tests

    [Fact]
    public void BanglalinkAuthenticationException_WithMessage_ShouldCreateException()
    {
        // Arrange
        const string message = "Authentication failed: Invalid credentials";

        // Act
        var exception = new BanglalinkAuthenticationException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Should().BeOfType<BanglalinkClientException>();
    }

    [Fact]
    public void BanglalinkAuthenticationException_InheritsFromBanglalinkClientException()
    {
        // Arrange & Act
        var exception = new BanglalinkAuthenticationException("Test");

        // Assert
        exception.Should().BeAssignableTo<BanglalinkClientException>();
    }

    [Fact]
    public void BanglalinkAuthenticationException_WithMessageAndStatusCode_ShouldPreserveInfo()
    {
        // Arrange
        const string message = "Authentication failed";

        // Act
        var exception = new BanglalinkAuthenticationException(message, new HttpRequestException());

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().NotBeNull();
    }

    #endregion

    #region Exception Hierarchy Tests

    [Fact]
    public void AllCustomExceptions_ShouldInheritFromBanglalinkClientException()
    {
        // Arrange & Act
        var configException = new BanglalinkConfigurationException("Config error");
        var authException = new BanglalinkAuthenticationException("Auth error");

        // Assert
        configException.Should().BeAssignableTo<BanglalinkClientException>();
        authException.Should().BeAssignableTo<BanglalinkClientException>();
    }

    [Fact]
    public void BanglalinkClientException_InheritsFromException()
    {
        // Arrange & Act
        var exception = new BanglalinkClientException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region Exception Catching Tests

    [Fact]
    public void BanglalinkConfigurationException_CanBeCaughtAsBaseException()
    {
        // Arrange
        var exception = new BanglalinkConfigurationException("Config error");

        // Act & Assert
        Action throwAction = () => throw exception;
        throwAction.Should().Throw<BanglalinkClientException>();
    }

    [Fact]
    public void BanglalinkAuthenticationException_CanBeCaughtAsBaseException()
    {
        // Arrange
        var exception = new BanglalinkAuthenticationException("Auth error");

        // Act & Assert
        Action throwAction = () => throw exception;
        throwAction.Should().Throw<BanglalinkClientException>();
    }

    #endregion

    #region Exception Serialization Tests

    [Fact]
    public void BanglalinkClientException_WithMessage_ShouldSerializeCorrectly()
    {
        // Arrange
        const string message = "Test exception message";
        var exception = new BanglalinkClientException(message);

        // Act
        var exceptionMessage = exception.ToString();

        // Assert
        exceptionMessage.Should().Contain(message);
    }

    [Fact]
    public void BanglalinkConfigurationException_WithMessage_ShouldSerializeCorrectly()
    {
        // Arrange
        const string message = "Configuration exception message";
        var exception = new BanglalinkConfigurationException(message);

        // Act
        var exceptionMessage = exception.ToString();

        // Assert
        exceptionMessage.Should().Contain(message);
    }

    #endregion
}
