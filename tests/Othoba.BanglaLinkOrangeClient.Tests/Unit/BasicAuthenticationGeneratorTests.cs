using FluentAssertions;
using Othoba.BanglaLinkOrange.Utilities;
using Xunit;

namespace Othoba.BanglaLinkOrange.Tests.Unit;

/// <summary>
/// Unit tests for BasicAuthenticationGenerator utility class.
/// Tests Base64 encoding of credentials for Basic Auth.
/// </summary>
public class BasicAuthenticationGeneratorTests
{
    #region Valid Credential Tests

    [Fact]
    public void GenerateToken_WithValidCredentials_ShouldReturnBase64EncodedToken()
    {
        // Arrange
        const string clientId = "my-client-id";
        const string clientSecret = "my-client-secret";

        // Act
        var token = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Verify it's valid Base64 by decoding
        var decodedBytes = Convert.FromBase64String(token);
        var decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);
        decoded.Should().Be($"{clientId}:{clientSecret}");
    }

    [Fact]
    public void GenerateToken_WithSpecialCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        const string clientId = "client-id-with-special!@#";
        const string clientSecret = "secret-with-$pecial&chars";

        // Act
        var token = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var decodedBytes = Convert.FromBase64String(token);
        var decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);
        decoded.Should().Be($"{clientId}:{clientSecret}");
    }

    [Fact]
    public void GenerateToken_WithLongCredentials_ShouldEncodeCorrectly()
    {
        // Arrange
        const string clientId = "very-long-client-id-that-has-many-characters-in-it-12345-abcde-fghij-klmno-pqrst-uvwxyz";
        const string clientSecret = "very-long-client-secret-that-has-many-characters-in-it-12345-abcde-fghij-klmno-pqrst-uvwxyz";

        // Act
        var token = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var decodedBytes = Convert.FromBase64String(token);
        var decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);
        decoded.Should().Be($"{clientId}:{clientSecret}");
    }

    [Fact]
    public void GenerateToken_WithSimpleCredentials_ShouldReturnCorrectBase64()
    {
        // Arrange
        const string clientId = "admin";
        const string clientSecret = "password";

        // Act
        var token = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);

        // Assert
        // "admin:password" in Base64 is "YWRtaW46cGFzc3dvcmQ="
        token.Should().Be("YWRtaW46cGFzc3dvcmQ=");
    }

    #endregion

    #region Null/Empty Credential Tests

    [Fact]
    public void GenerateToken_WithNullClientId_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? clientId = null;
        const string clientSecret = "secret";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BasicAuthenticationGenerator.GenerateToken(clientId!, clientSecret));
    }

    [Fact]
    public void GenerateToken_WithNullClientSecret_ShouldThrowArgumentNullException()
    {
        // Arrange
        const string clientId = "client";
        string? clientSecret = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret!));
    }

    [Fact]
    public void GenerateToken_WithEmptyClientId_ShouldThrowArgumentNullException()
    {
        // Arrange
        const string clientId = "";
        const string clientSecret = "secret";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret));
    }

    [Fact]
    public void GenerateToken_WithEmptyClientSecret_ShouldThrowArgumentNullException()
    {
        // Arrange
        const string clientId = "client";
        const string clientSecret = "";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret));
    }

    #endregion

    #region Token Format Tests

    [Fact]
    public void GenerateToken_ShouldReturnBase64FormattedString()
    {
        // Arrange
        const string clientId = "test";
        const string clientSecret = "secret";

        // Act
        var token = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);

        // Assert
        // Valid Base64 string should not throw when decoded
        var action = () => Convert.FromBase64String(token);
        action.Should().NotThrow();
    }

    [Fact]
    public void GenerateToken_ShouldContainColon_AfterDecoding()
    {
        // Arrange
        const string clientId = "id";
        const string clientSecret = "secret";

        // Act
        var token = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);
        var decodedBytes = Convert.FromBase64String(token);
        var decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);

        // Assert
        decoded.Should().Contain(":");
        decoded.Should().StartWith(clientId);
        decoded.Should().EndWith(clientSecret);
    }

    #endregion

    #region Deterministic Tests

    [Fact]
    public void GenerateToken_WithSameInput_ShouldReturnSameOutput()
    {
        // Arrange
        const string clientId = "client-id";
        const string clientSecret = "client-secret";

        // Act
        var token1 = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);
        var token2 = BasicAuthenticationGenerator.GenerateToken(clientId, clientSecret);

        // Assert
        token1.Should().Be(token2);
    }

    [Fact]
    public void GenerateToken_WithDifferentInput_ShouldReturnDifferentOutput()
    {
        // Arrange
        var token1 = BasicAuthenticationGenerator.GenerateToken("client-1", "secret-1");
        var token2 = BasicAuthenticationGenerator.GenerateToken("client-2", "secret-2");

        // Act & Assert
        token1.Should().NotBe(token2);
    }

    #endregion
}
