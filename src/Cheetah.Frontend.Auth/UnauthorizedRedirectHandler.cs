using System.Net;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Frontend.Auth;

/// <summary>
/// Intercepts 401 Unauthorized responses during an active session,
/// clears the stored token, and triggers an auth state change so that
/// AuthorizeRouteView redirects the user to /login.
/// Only fires when a token is present — avoids spurious logout signals
/// on 401s from the login endpoint itself (wrong password, etc.).
/// </summary>
[Export(LifetimeType.Transient, typeof(UnauthorizedRedirectHandler))]
public sealed class UnauthorizedRedirectHandler : DelegatingHandler
{
    private readonly IAuthStateNotifier _notifier;
    private readonly ITokenStorage _tokenStorage;

    public UnauthorizedRedirectHandler(IAuthStateNotifier notifier, ITokenStorage tokenStorage)
    {
        _notifier = notifier;
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var token = await _tokenStorage.GetTokenAsync(cancellationToken);
            if (!string.IsNullOrEmpty(token))
                await _notifier.NotifyLogoutAsync();
        }

        return response;
    }
}
