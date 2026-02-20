using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.Identity.DataAccess.Context;
using Cheetah.Core.Identity.DataAccess.Services;
using Cheetah.Core.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.Identity.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IdentityBuilder AddIdentityContext<TContext, TIdentityUser, TIdentityRole>(this IServiceCollection services, Action<IdentityOptions> setupAction)
        where TContext : IdentityDbContext<TContext, TIdentityUser, TIdentityRole> where TIdentityRole : IdentityRole where TIdentityUser : IdentityUser<TIdentityRole>
    {
        services.AddApplicationDbContext<TContext>();

        services.TryAddScoped<IdentityRoleManager<TIdentityRole>>();
        services.TryAddScoped(typeof(RoleManager<TIdentityRole>), provider => provider.GetRequiredService(typeof(IdentityRoleManager<TIdentityRole>)));

        services.TryAddScoped<IdentityUserManager<TIdentityUser, TIdentityRole>>();
        services.TryAddScoped(typeof(UserManager<TIdentityUser>), provider => provider.GetRequiredService(typeof(IdentityUserManager<TIdentityUser, TIdentityRole>)));

        services.TryAddScoped<IdentityUserStore<TIdentityUser, TIdentityRole, TContext>>();
        services.TryAddScoped(typeof(IUserStore<TIdentityUser>), provider => provider.GetRequiredService(typeof(IdentityUserStore<TIdentityUser, TIdentityRole, TContext>)));

        services.TryAddScoped<IdentityRoleStore<TIdentityRole, TContext>>();
        services.TryAddScoped(typeof(IRoleStore<TIdentityRole>), provider => provider.GetRequiredService(typeof(IdentityRoleStore<TIdentityRole, TContext>)));

        return services
            .AddIdentityCore<TIdentityUser>(setupAction)
            .AddRoles<TIdentityRole>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory<TIdentityUser, TIdentityRole>>();
    }
}