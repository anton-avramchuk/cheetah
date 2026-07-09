using Cheetah.Audit;
using Cheetah.Core.Domain;

namespace Cheetah.Modules.FeatureManagement.Domain.Entities;

/// <summary>Вариант A/B (child агрегата) с весом для детерминированного распределения.</summary>
[Auditable]
public sealed class FeatureVariantDef : Entity<Guid>
{
    public Guid FlagId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Value { get; private set; }
    public int Weight { get; private set; }

    private FeatureVariantDef() { }

    public static FeatureVariantDef Create(Guid flagId, string name, string? value, int weight)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new FeatureVariantDef
        {
            Id = Guid.NewGuid(),
            FlagId = flagId,
            Name = name.Trim(),
            Value = value,
            Weight = Math.Max(0, weight)
        };
    }
}
