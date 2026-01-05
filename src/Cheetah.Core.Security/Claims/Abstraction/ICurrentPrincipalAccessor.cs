using System.Security.Claims;

namespace Cheetah.Core.Security.Claims.Abstraction;

public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal Principal { get; }

    IDisposable Change(ClaimsPrincipal principal);
}