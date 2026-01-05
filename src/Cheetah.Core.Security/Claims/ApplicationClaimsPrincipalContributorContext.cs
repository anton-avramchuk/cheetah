using System.Security.Claims;

namespace Cheetah.Core.Security.Claims;

public class ApplicationClaimsPrincipalContributorContext(
    ClaimsPrincipal claimsIdentity,
    IServiceProvider serviceProvider)
{
    public ClaimsPrincipal ClaimsIdentity { get; } = claimsIdentity ?? throw new ArgumentNullException(nameof(claimsIdentity));
    public IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
}