using System.Net;
using Moq;
using Moq.Protected;
using Othoba.BanglaLinkOrange.Configuration;

namespace Othoba.BanglaLinkOrange.Tests.Fixtures;

/// <summary>
/// Fixture for creating test data and mocks for BanglalinkAuthClient tests.
/// </summary>
public class BanglalinkAuthClientFixture
{
    public const string TestBaseUrl = "http://localhost:8080";
    public const string TestClientId = "test-client-id";
    public const string TestClientSecret = "test-client-secret";
    public const string TestUsername = "test-user";
    public const string TestPassword = "test-password";
    public const string TestAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    public const string TestRefreshToken = "refresh-token-value";

    public BanglalinkClientConfiguration CreateValidConfiguration()
    {
        return new BanglalinkClientConfiguration
        {
            BaseUrl = TestBaseUrl,
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            Username = TestUsername,
            Password = TestPassword,
            Scope = "openid"
        };
    }

    public BanglalinkClientConfiguration CreateInvalidConfiguration()
    {
        return new BanglalinkClientConfiguration
        {
            BaseUrl = null!,
            ClientId = null!,
            ClientSecret = null!,
            Username = null!,
            Password = null!
        };
    }

    public string CreateValidTokenResponse()
    {
        return $$"""
        {
            "access_token": "{{TestAccessToken}}",
            "token_type": "Bearer",
            "expires_in": 36000,
            "refresh_token": "{{TestRefreshToken}}",
            "refresh_expires_in": 36000
        }
        """;
    }

    public string CreateErrorTokenResponse()
    {
        return $$"""
        {
            "error": "invalid_grant",
            "error_description": "Invalid credentials"
        }
        """;
    }

    public Mock<HttpMessageHandler> CreateMockHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mockHandler;
    }

    public HttpClient CreateHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = CreateMockHttpMessageHandler(responseContent, statusCode);
        return new HttpClient(mockHandler.Object)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
}
