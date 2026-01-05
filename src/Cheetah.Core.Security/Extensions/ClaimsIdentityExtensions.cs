using System.Security.Claims;
using System.Security.Principal;
using Cheetah.Core.Extensions.Common;
using Cheetah.Core.Security.Claims;

namespace Cheetah.Core.Security.Extensions;

public static class ClaimsIdentityExtensions
{
    public static Guid? FindUserId(this ClaimsPrincipal principal)
    {

        var userIdOrNull = principal.Claims?.FirstOrDefault(c => c.Type == ApplicationClaimTypes.UserId);
        if (userIdOrNull == null || userIdOrNull.Value.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (Guid.TryParse(userIdOrNull.Value, out Guid guid))
        {
            return guid;
        }

        return null;
    }

    public static Guid? FindUserId(this IIdentity identity)
    {

        var claimsIdentity = identity as ClaimsIdentity;

        var claims = claimsIdentity?.Claims?.Where(c => c.Type == ApplicationClaimTypes.UserId).ToArray();


        if (claims != null)
            foreach (var claim in claims)
            {
                if (claim.Value.IsNullOrWhiteSpace())
                    continue;
                if (Guid.TryParse(claim.Value, out var guid))
                {
                    return guid;
                }
            }


        return null;
    }





    public static ClaimsIdentity AddIfNotContains(this ClaimsIdentity claimsIdentity, Claim claim)
    {

        if (!claimsIdentity.Claims.Any(x => string.Equals(x.Type, claim.Type, StringComparison.OrdinalIgnoreCase)))
        {
            claimsIdentity.AddClaim(claim);
        }

        return claimsIdentity;
    }

    public static ClaimsIdentity AddOrReplace(this ClaimsIdentity claimsIdentity, Claim claim)
    {

        foreach (var x in claimsIdentity.FindAll(claim.Type).ToList())
        {
            claimsIdentity.RemoveClaim(x);
        }

        claimsIdentity.AddClaim(claim);

        return claimsIdentity;
    }

    public static ClaimsPrincipal AddIdentityIfNotContains(this ClaimsPrincipal principal, ClaimsIdentity identity)
    {

        if (!principal.Identities.Any(x => string.Equals(x.AuthenticationType, identity.AuthenticationType, StringComparison.OrdinalIgnoreCase)))
        {
            principal.AddIdentity(identity);
        }

        return principal;
    }

}