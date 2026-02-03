namespace Cheetah.Blazor.Layout.Abstractions;

/// <summary>
/// Interface for components that can be rendered in the header.
/// Register implementations to add custom components to the header area.
/// </summary>
public interface IHeaderComponent
{
    /// <summary>
    /// Order of the component in the header. Lower values appear first.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// The component type to render.
    /// </summary>
    Type ComponentType { get; }

    /// <summary>
    /// Optional parameters to pass to the component.
    /// </summary>
    IDictionary<string, object>? Parameters => null;
}
