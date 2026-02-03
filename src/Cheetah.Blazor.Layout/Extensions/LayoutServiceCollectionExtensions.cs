using Cheetah.Blazor.Layout.Abstractions;
using Cheetah.Blazor.Layout.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Blazor.Layout.Extensions;

/// <summary>
/// Extension methods for configuring the CRM layout.
/// </summary>
public static class LayoutServiceCollectionExtensions
{
    /// <summary>
    /// Configures the CRM layout with custom settings.
    /// </summary>
    public static IServiceCollection ConfigureCrmLayout(
        this IServiceCollection services,
        Action<LayoutConfiguration> configure)
    {
        var config = new LayoutConfiguration();
        configure(config);
        services.AddSingleton<LayoutConfiguration>(config);
        services.AddSingleton<ILayoutConfiguration>(config);
        return services;
    }

    /// <summary>
    /// Adds a header component to be rendered in the header area.
    /// </summary>
    public static IServiceCollection AddHeaderComponent<TComponent>(
        this IServiceCollection services,
        int order = 0,
        IDictionary<string, object>? parameters = null)
        where TComponent : class, Microsoft.AspNetCore.Components.IComponent
    {
        services.AddSingleton<IHeaderComponent>(new HeaderComponentRegistration(
            typeof(TComponent),
            order,
            parameters
        ));
        return services;
    }

    private class HeaderComponentRegistration : IHeaderComponent
    {
        public HeaderComponentRegistration(Type componentType, int order, IDictionary<string, object>? parameters)
        {
            ComponentType = componentType;
            Order = order;
            Parameters = parameters;
        }

        public int Order { get; }
        public Type ComponentType { get; }
        public IDictionary<string, object>? Parameters { get; }
    }
}
