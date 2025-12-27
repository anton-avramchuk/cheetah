namespace Cheetah.Core.Modularity;

/// <summary>
/// Attribute to declare module dependencies
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute(params Type[] dependencies) : Attribute
{
    public Type[] Dependencies { get; } = dependencies;
}