using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributorProvider))]
public class MenuContributorProvider : IMenuContributorProvider
{
    private readonly IEnumerable<IMenuContributor> _contributors;

    public MenuContributorProvider(IEnumerable<IMenuContributor> contributors)
    {
        _contributors = contributors;
    }

    public IEnumerable<IMenuContributor> GetContributors(string menuId)
        => _contributors.Where(c => c.TargetMenuId == menuId);
}
