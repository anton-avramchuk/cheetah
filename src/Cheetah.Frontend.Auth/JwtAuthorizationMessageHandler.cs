using System.Net.Http.Headers;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Frontend.Auth;

[Export(LifetimeType.Transient, typeof(JwtAuthorizationMessageHandler))]
public sealed class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;

    public JwtAuthorizationMessageHandler(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetTokenAsync(cancellationToken);

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
