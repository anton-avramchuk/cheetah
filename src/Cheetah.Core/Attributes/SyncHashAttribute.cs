namespace Cheetah.Core.Attributes;

/// <summary>
/// Marks a property or record parameter as part of the sync hash computation.
/// Used by Cheetah.Generators.Common to generate ComputeHash / ComputeBatchHash extension methods.
/// </summary>
/// <param name="order">Determines the order in which fields are concatenated before hashing. Lower values come first.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SyncHashAttribute(int order = 0) : Attribute
{
    public int Order { get; } = order;
}
