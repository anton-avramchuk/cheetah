using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Tags.Contracts.Registry;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;

namespace Cheetah.Modules.Tags.Application.Registry;

/// <summary>Список зарегистрированных применимых типов (опц. фильтр по сервису-владельцу).</summary>
public sealed record ListTaggableTypesQuery(string? OwnerService = null) : IQuery<IReadOnlyList<TaggableEntityTypeDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListTaggableTypesQuery, IReadOnlyList<TaggableEntityTypeDto>>))]
public class ListTaggableTypesQueryHandler
    : IQueryHandler<ListTaggableTypesQuery, IReadOnlyList<TaggableEntityTypeDto>>
{
    private readonly IRepository<TaggableEntityType, string> _repository;

    public ListTaggableTypesQueryHandler(IRepository<TaggableEntityType, string> repository)
        => _repository = repository;

    public async ValueTask<IReadOnlyList<TaggableEntityTypeDto>> HandleAsync(
        ListTaggableTypesQuery query, CancellationToken ct = default)
    {
        var items = string.IsNullOrEmpty(query.OwnerService)
            ? await _repository.GetAllAsync(null, ct)
            : await _repository.GetAllAsync(new TaggableTypesByOwnerSpecification(query.OwnerService), ct);

        return items
            .OrderBy(t => t.Id, StringComparer.Ordinal)
            .Select(Map)
            .ToArray();
    }

    internal static TaggableEntityTypeDto Map(TaggableEntityType t)
        => new(t.Id, t.DisplayName, t.OwnerService,
            t.MaxTagsPerEntity, t.AllowAdHocTags, t.AllowedGroups.ToArray());
}
