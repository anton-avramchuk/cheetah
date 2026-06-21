namespace Cheetah.AspNetCore.Blazor.Navigation;

public interface IMenuContributorProvider
{
    IEnumerable<IMenuContributor> GetContributors(string menuId);
}
