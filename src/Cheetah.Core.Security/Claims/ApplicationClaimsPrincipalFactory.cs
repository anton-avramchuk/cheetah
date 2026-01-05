using System.Security.Claims;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Security.Claims.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Security.Claims;

[Export(LifetimeType.Transient,typeof(IApplicationClaimsPrincipalFactory))]
public class ApplicationClaimsPrincipalFactory(IServiceScopeFactory serviceScopeFactory)
    : IApplicationClaimsPrincipalFactory
{
    public IServiceScopeFactory ServiceScopeFactory { get; } = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    public Task<ClaimsPrincipal> CreateAsync(ClaimsPrincipal existsClaimsPrincipal = null)
    {
        using (var scope = ServiceScopeFactory.CreateScope())
        {
            var claimsPrincipal = existsClaimsPrincipal ?? new ClaimsPrincipal(new ClaimsIdentity(
                "Application",
                ApplicationClaimTypes.UserName,
                ApplicationClaimTypes.Role));

            var context = new ApplicationClaimsPrincipalContributorContext(claimsPrincipal, scope.ServiceProvider);


            return Task.FromResult(claimsPrincipal);
            //foreach (var contributorType in Options.Contributors)
            //{
            //    var contributor = (IAbpClaimsPrincipalContributor)scope.ServiceProvider.GetRequiredService(contributorType);
            //    await contributor.ContributeAsync(context);
            //}

            //return claimsPrincipal;
        }
    }
}