using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Othoba.BanglaLinkOrange.Clients;

namespace Othoba.BanglaLinkOrange.Handlers;

/// <summary>
/// DelegatingHandler that automatically injects Bearer token into requests.
/// Retrieves the token from the registered IBanglalinkAuthClient.
/// </summary>
public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IBanglalinkAuthClient? _authClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationDelegatingHandler"/> class.
    /// </summary>
    /// <param name="authClient">The authentication client for token retrieval.</param>
    public AuthenticationDelegatingHandler(IBanglalinkAuthClient? authClient)
    {
        _authClient = authClient;
    }

    /// <summary>
    /// Sends the HTTP request with automatic Bearer token injection.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only add token if auth client is available and no authorization header already exists
        if (_authClient != null && !request.Headers.Contains("Authorization"))
        {
            var accessToken = await _authClient.GetValidAccessTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
