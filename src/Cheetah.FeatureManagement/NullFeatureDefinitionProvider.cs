namespace Cheetah.FeatureManagement;

/// <summary>
/// Безопасный провайдер-заглушка: ни один флаг не зарегистрирован, всё оценивается как выключено.
/// Регистрируется <see cref="CrmFeatureManagementModule"/> через <c>TryAdd</c> — фактический источник
/// определений (Infrastructure — БД+кэш, Client — реплика) регистрируется later/поверх обычным <c>Add</c>
/// и побеждает при резолве (последняя регистрация выигрывает).
/// <para>
/// Нужен, чтобы модуль-абстракция (<see cref="IFeatureManager"/>) не ронял DI-валидацией любой хост,
/// который лишь транзитивно зависит от неё (например, через <c>Cheetah.Backend.Endpoints</c> и его
/// <c>RequireFeature</c>-гейт), но сам не подключает ни один конкретный источник флагов.
/// </para>
/// </summary>
public sealed class NullFeatureDefinitionProvider : IFeatureDefinitionProvider
{
    public ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult<FeatureDefinition?>(null);

    public ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult<IReadOnlyList<FeatureDefinition>>([]);
}
