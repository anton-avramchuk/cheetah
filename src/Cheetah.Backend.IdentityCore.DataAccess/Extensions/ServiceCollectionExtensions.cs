using Cheetah.Backend.IdentityCore.DataAccess.Services;
using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.EntityFramework.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Backend.IdentityCore.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IdentityBuilder AddIdentityContext<TContext, TIdentityUser, TIdentityRole>(this IServiceCollection services, Action<IdentityOptions> setupAction)
        where TContext : IdentityDbContext<TContext, TIdentityUser, TIdentityRole> where TIdentityRole : IdentityRole where TIdentityUser : IdentityUser<TIdentityRole>
    {
        services.AddApplicationDbContext<TContext>();

        services.TryAddScoped<IdentityRoleManager<TIdentityRole>>();
        services.TryAddScoped(typeof(RoleManager<TIdentityRole>), provider => provider.GetService(typeof(IdentityRoleManager<TIdentityRole>)));

        services.TryAddScoped<IdentityUserManager<TIdentityUser, TIdentityRole>>();
        services.TryAddScoped(typeof(UserManager<TIdentityUser>), provider => provider.GetService(typeof(IdentityUserManager<TIdentityUser, TIdentityRole>)));

        services.TryAddScoped<IdentityUserStore<TIdentityUser, TIdentityRole, TContext>>();
        services.TryAddScoped(typeof(IUserStore<TIdentityUser>), provider => provider.GetService(typeof(IdentityUserStore<TIdentityUser, TIdentityRole, TContext>)));

        services.TryAddScoped<IdentityRoleStore<TIdentityRole, TContext>>();
        services.TryAddScoped(typeof(IRoleStore<TIdentityRole>), provider => provider.GetService(typeof(IdentityRoleStore<TIdentityRole, TContext>)));



        return services
                .AddIdentityCore<TIdentityUser>(setupAction)
                .AddRoles<TIdentityRole>()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory<TIdentityUser, TIdentityRole>>()
            ;
    }
}