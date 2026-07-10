using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.FeatureManagement;
using Cheetah.Permissions.Catalog.Domain;

namespace Cheetah.Permissions.Catalog.Application;

/// <summary>
/// Список всех permissions в каталоге. Опционально фильтрация по модулю.
/// Для UI: админ выбирает permission, чтобы навесить на роль / пользователя.
/// <para>
/// Permissions, привязанные к фиче (<see cref="PermissionDefinition.Feature"/>), отдаются, только пока
/// эта фича включена. Незаведённый флаг <c>IFeatureManager</c> трактует как выключенный, поэтому
/// «фичи нет» и «фича выключена» ведут себя одинаково: назначать право на несуществующую
/// функциональность нечего.
/// </para>
/// </summary>
public sealed record ListPermissionsQuery(string? Module = null) : IQuery<IReadOnlyList<PermissionDefinitionDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionDefinitionDto>>))]
public class ListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionDefinitionDto>>
{
    private readonly IRepository<PermissionDefinition, string> _repository;
    private readonly IFeatureManager _features;

    public ListPermissionsQueryHandler(
        IRepository<PermissionDefinition, string> repository, IFeatureManager features)
    {
        _repository = repository;
        _features = features;
    }

    public async ValueTask<IReadOnlyList<PermissionDefinitionDto>> HandleAsync(
        ListPermissionsQuery query, CancellationToken ct = default)
    {
        var items = string.IsNullOrEmpty(query.Module)
            ? await _repository.GetAllAsync(null, ct)
            : await _repository.GetAllAsync(new PermissionsByModuleSpecification(query.Module), ct);

        var disabled = await CollectDisabledFeaturesAsync(items, ct);

        return items
            .Where(p => p.Feature is null || !disabled.Contains(p.Feature))
            .OrderBy(p => p.Module, StringComparer.Ordinal)
            .ThenBy(p => p.Id, StringComparer.Ordinal)
            .Select(p => new PermissionDefinitionDto(p.Id, p.Description, p.Module, p.Feature))
            .ToArray();
    }

    /// <summary>Спрашивает движок один раз на каждую РАЗЛИЧНУЮ фичу, а не на каждый permission.</summary>
    private async ValueTask<HashSet<string>> CollectDisabledFeaturesAsync(
        IEnumerable<PermissionDefinition> items, CancellationToken ct)
    {
        var features = items
            .Select(p => p.Feature)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(f => f, StringComparer.Ordinal);

        var disabled = new HashSet<string>(StringComparer.Ordinal);
        foreach (var feature in features)
            if (!await _features.IsEnabledAsync(feature, ct: ct))
                disabled.Add(feature);

        return disabled;
    }
}
