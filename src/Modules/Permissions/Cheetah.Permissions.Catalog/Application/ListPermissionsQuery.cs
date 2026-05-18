using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Permissions.Catalog.Domain;

namespace Cheetah.Permissions.Catalog.Application;

/// <summary>
/// Список всех permissions в каталоге. Опционально фильтрация по модулю.
/// Для UI: админ выбирает permission, чтобы навесить на роль / пользователя.
/// </summary>
public sealed record ListPermissionsQuery(string? Module = null) : IQuery<IReadOnlyList<PermissionDefinitionDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionDefinitionDto>>))]
public class ListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionDefinitionDto>>
{
    private readonly IRepository<PermissionDefinition, string> _repository;

    public ListPermissionsQueryHandler(IRepository<PermissionDefinition, string> repository)
        => _repository = repository;

    public async ValueTask<IReadOnlyList<PermissionDefinitionDto>> HandleAsync(
        ListPermissionsQuery query, CancellationToken ct = default)
    {
        var items = string.IsNullOrEmpty(query.Module)
            ? await _repository.GetAllAsync(null, ct)
            : await _repository.GetAllAsync(new PermissionsByModuleSpecification(query.Module), ct);

        return items
            .OrderBy(p => p.Module, StringComparer.Ordinal)
            .ThenBy(p => p.Id, StringComparer.Ordinal)
            .Select(p => new PermissionDefinitionDto(p.Id, p.Description, p.Module))
            .ToArray();
    }
}
