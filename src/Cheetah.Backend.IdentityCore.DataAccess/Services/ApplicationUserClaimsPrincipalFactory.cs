using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.Security.Claims.Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.IdentityCore.DataAccess.Services;

public class ApplicationUserClaimsPrincipalFactory<TIdentityUser, TIdentityRole> : 
    UserClaimsPrincipalFactory<TIdentityUser, TIdentityRole>
    where TIdentityRole : IdentityRole
    where TIdentityUser : IdentityUser<TIdentityRole>
{
    public ICurrentPrincipalAccessor CurrentPrincipalAccessor { get; }

    public ApplicationUserClaimsPrincipalFactory(
        UserManager<TIdentityUser> userManager,
        RoleManager<TIdentityRole> roleManager,
        IOptions<IdentityOptions> options,
        ICurrentPrincipalAccessor currentPrincipalAccessor
    ) : base(userManager, roleManager, options)
    {
        CurrentPrincipalAccessor = currentPrincipalAccessor;
    }
}