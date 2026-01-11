using Cheetah.Backend.IdentityCore.Api.Options;

namespace Cheetah.Backend.IdentityCore.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseIdentity<TOptions, TIdentityUsersOptions, TIdentityRolesOptions>(this IApplicationBuilder app,
        TOptions options)
        where TOptions : IdentityApiOptions<TIdentityUsersOptions, TIdentityRolesOptions>
        where TIdentityRolesOptions : IdentityRolesOptions
        where TIdentityUsersOptions : IdentityUsersOptions
    {
        if (options.Users != null)
        {
            app.AddUsers(options.Users);
        }

        if (options.Roles != null)
        {
            app.AddRoles(options.Roles);
        }
    }

    private static void AddUsers<TIdentityUsersOptions>(this IApplicationBuilder app, TIdentityUsersOptions options)
        where TIdentityUsersOptions : IdentityUsersOptions
    {
    }


    private static void AddRoles<TIdentityRolesOptions>(this IApplicationBuilder app, TIdentityRolesOptions options)
        where TIdentityRolesOptions : IdentityRolesOptions
    {
    }
}