using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

/// <summary>AuthenticationStateProvider с фиксированным принципалом для тестов.</summary>
internal sealed class FakeAuthStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
{
    public static FakeAuthStateProvider WithSession(string sessionId)
    {
        var identity = new ClaimsIdentity([new Claim(BffClaimTypes.SessionId, sessionId)], "Test");
        return new FakeAuthStateProvider(new ClaimsPrincipal(identity));
    }

    public static FakeAuthStateProvider Anonymous() => new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(principal));
}

/// <summary>Управляемые часы для тестов истечения токена.</summary>
internal sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;
    public override DateTimeOffset GetUtcNow() => Now;
}
