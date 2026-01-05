using System.Security.Claims;

namespace Cheetah.Core.Security.Claims.Abstraction;

public interface IApplicationClaimsPrincipalFactory
{
    Task<ClaimsPrincipal> CreateAsync(ClaimsPrincipal? existsClaimsPrincipal = null);
}