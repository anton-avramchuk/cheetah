using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.IdentityCore.Api.Services;

public class IdentitySignInManager<TIdentityUser, TIdentityRole> : SignInManager<TIdentityUser>
    where TIdentityUser : Cheetah.Backend.IdentityCore.Domain.IdentityUser<TIdentityRole>
    where TIdentityRole : Cheetah.Backend.IdentityCore.Domain.IdentityRole
{
    public IdentitySignInManager(UserManager<TIdentityUser> userManager, IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<TIdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<TIdentityUser>> logger, IAuthenticationSchemeProvider schemes,
        IUserConfirmation<TIdentityUser> confirmation) : base(userManager, contextAccessor, claimsFactory,
        optionsAccessor, logger, schemes, confirmation)
    {
    }
}