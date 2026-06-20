namespace Cheetah.FeatureManagement;

/// <summary>
/// Контекст оценки фич-флага: кто и где спрашивает. Заполняется из HTTP-контекста потребителя
/// или вручную. Передаётся во встроенные и пользовательские <see cref="IFeatureFilter"/>.
/// </summary>
public sealed record FeatureContext
{
    /// <summary>Пользователь, для которого вычисляется флаг (стабилизация percentage-rollout).</summary>
    public Guid? UserId { get; init; }

    /// <summary>Тенант, для которого вычисляется флаг (tenant-override + стабилизация rollout).</summary>
    public Guid? TenantId { get; init; }

    /// <summary>Роли субъекта (фильтр <c>Roles</c>).</summary>
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>Произвольные атрибуты для JsonLogic-условий и пользовательских фильтров.</summary>
    public IReadOnlyDictionary<string, object?> Attributes { get; init; }
        = new Dictionary<string, object?>();

    /// <summary>Пустой контекст (анонимная оценка — только глобальные правила/percentage без субъекта).</summary>
    public static FeatureContext Empty { get; } = new();
}
