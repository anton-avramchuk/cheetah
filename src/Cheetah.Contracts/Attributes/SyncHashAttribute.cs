namespace Cheetah.Contracts.Attributes;

/// <summary>
/// Marks a property or record parameter as part of the sync hash computation.
/// Used by Cheetah.Generators.Common to generate ComputeHash / ComputeBatchHash extension methods.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SyncHashAttribute : Attribute;
