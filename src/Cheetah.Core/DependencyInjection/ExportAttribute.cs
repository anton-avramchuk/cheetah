namespace Cheetah.Core.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ExportAttribute(LifetimeType lifetime, params Type[] types) : Attribute
{
    public LifetimeType Lifetime { get; set; } = lifetime;
    public Type[] Types { get; } = types;
}