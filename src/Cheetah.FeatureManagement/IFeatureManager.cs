namespace Cheetah.FeatureManagement;

/// <summary>
/// Единая точка ответа на вопрос «включена ли фича X в данном контексте?».
/// Нужен каждому модулю-потребителю, поэтому живёт в лёгкой абстракции без БД.
/// Горячий путь: работает над уже закэшированными/реплицированными определениями
/// (<see cref="IFeatureDefinitionProvider"/>), без I/O на запрос.
/// </summary>
public interface IFeatureManager
{
    /// <summary>Включён ли булев флаг (или флаг-вариант, если сработало любое правило).</summary>
    ValueTask<bool> IsEnabledAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default);

    /// <summary>Выбранный вариант флага-варианта; <c>null</c>, если флаг выключен или не вариативный.</summary>
    ValueTask<FeatureVariant?> GetVariantAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default);
}

/// <summary>Результат флага-варианта: имя варианта и опциональная полезная нагрузка.</summary>
public sealed record FeatureVariant(string Name, string? Value);
