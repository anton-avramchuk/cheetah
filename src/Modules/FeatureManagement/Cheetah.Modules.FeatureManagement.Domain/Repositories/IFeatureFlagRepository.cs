using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Domain.Repositories;

/// <summary>
/// Репозиторий флага с загрузкой агрегата вместе с детьми (правила/варианты/override). Нужен, чтобы
/// Application мог заменять набор правил (удаление сирот) без знания EF. Реализация — в Infrastructure.
/// </summary>
public interface IFeatureFlagRepository<TFlag> : IRepository<TFlag, Guid>
    where TFlag : FeatureFlagBase
{
    /// <summary>Флаг по ключу; <paramref name="includeChildren"/> подгружает правила/варианты/override.</summary>
    ValueTask<TFlag?> GetByKeyAsync(string key, bool includeChildren, CancellationToken ct = default);

    /// <summary>Список флагов по спецификации (опц.); <paramref name="includeChildren"/> — с детьми.</summary>
    ValueTask<IReadOnlyList<TFlag>> ListAsync(ISpecification<TFlag>? spec, bool includeChildren, CancellationToken ct = default);
}
