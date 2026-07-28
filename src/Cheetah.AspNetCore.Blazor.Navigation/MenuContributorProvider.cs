using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Отдаёт контрибьюторов нужного меню. Scoped: иначе scoped-контрибьюторы (те, что берут пункты
/// из данных текущего пользователя) не были бы разрешимы. Singleton-контрибьюторы продолжают
/// работать без изменений.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IMenuContributorProvider))]
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
