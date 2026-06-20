using Cheetah.Core.Domain;

namespace Cheetah.Modules.FeatureManagement.Domain.Entities;

/// <summary>
/// Правило таргетинга (child агрегата <see cref="FeatureFlagBase"/>). Конкретный тип: расширяемость
/// таргетинга достигается не наследованием правила, а plugin-фильтрами (<c>IFeatureFilter</c>) —
/// <see cref="FilterName"/> + произвольные <see cref="ParametersJson"/> уже открыты.
/// </summary>
public sealed class TargetingRule : Entity<Guid>
{
    public Guid FlagId { get; private set; }
    public int Order { get; private set; }
    public string FilterName { get; private set; } = null!;

    /// <summary>Параметры фильтра в JSON (хранятся как <c>jsonb</c>).</summary>
    public string ParametersJson { get; private set; } = "{}";

    /// <summary>Для флага-варианта: какой вариант отдать при срабатывании.</summary>
    public string? ResultVariant { get; private set; }

    /// <summary>Deny-правило: при срабатывании флаг выключается (короткое замыкание).</summary>
    public bool Negate { get; private set; }

    private TargetingRule() { }

    public static TargetingRule Create(Guid flagId, int order, string filterName,
        string parametersJson, string? resultVariant, bool negate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterName);
        return new TargetingRule
        {
            Id = Guid.NewGuid(),
            FlagId = flagId,
            Order = order,
            FilterName = filterName.Trim(),
            ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson,
            ResultVariant = resultVariant,
            Negate = negate
        };
    }
}
