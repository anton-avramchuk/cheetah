using Cheetah.Blazor.Layout.Abstractions;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Blazor.Layout.Services;

/// <summary>
/// Provides access to registered header components.
/// </summary>
public interface IHeaderComponentProvider
{
    /// <summary>
    /// Gets all registered header components ordered by their Order property.
    /// </summary>
    IEnumerable<IHeaderComponent> GetComponents();
}

/// <summary>
/// Default implementation that resolves header components from DI.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IHeaderComponentProvider))]
public class HeaderComponentProvider : IHeaderComponentProvider
{
    private readonly IEnumerable<IHeaderComponent> _components;

    public HeaderComponentProvider(IEnumerable<IHeaderComponent> components)
    {
        _components = components;
    }

    public IEnumerable<IHeaderComponent> GetComponents()
    {
        return _components.OrderBy(c => c.Order);
    }
}
