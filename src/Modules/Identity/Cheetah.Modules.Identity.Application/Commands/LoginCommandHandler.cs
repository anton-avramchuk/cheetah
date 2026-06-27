using System.Security.Claims;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public sealed class LoginCommandHandler<TUser, TRole>(
    UserManager<TUser> userManager,
    RoleManager<TRole> roleManager,
    ITokenGenerator tokenGenerator,
    IPasswordDecryptor passwordDecryptor)
    : ICommandHandler<LoginCommand, TokenResult>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask<TokenResult> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByNameAsync(command.UserName);
        if (user is null)
            throw new InvalidCredentialsException();

        var plainPassword = passwordDecryptor.Decrypt(command.Password);

        var isPasswordValid = await userManager.CheckPasswordAsync(user, plainPassword);
        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        var roles = await userManager.GetRolesAsync(user);

        var userName = user.UserName ?? throw new InvalidOperationException("User account is in an invalid state.");
        var email = user.Email ?? throw new InvalidOperationException("User account is in an invalid state.");

        var userClaims = await userManager.GetClaimsAsync(user);
        var rolesClaims = new List<Claim>();

        foreach (var role in roles)
        {
            var dbRole = await roleManager.FindByNameAsync(role);
            if (dbRole is null)
                continue;

            var roleClaims = await roleManager.GetClaimsAsync(dbRole);

            rolesClaims.AddRange(roleClaims);
        }

        var claims = rolesClaims.Union(userClaims).ToList();


        return tokenGenerator.GenerateToken(user.Id, userName, email, roles, claims);
    }
}