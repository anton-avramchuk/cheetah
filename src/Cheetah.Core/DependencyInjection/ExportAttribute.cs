namespace Cheetah.Core.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExportAttribute(LifetimeType lifetime, params Type[] types) : Attribute
{
    public LifetimeType Lifetime { get; set; } = lifetime;
    public Type[] Types { get; } = types;

    /// <summary>
    /// When set, registers the service as a keyed service using this key.
    /// Generates <c>AddKeyed*</c> instead of <c>Add*</c>.
    /// </summary>
    public string? Key { get; set; }
}